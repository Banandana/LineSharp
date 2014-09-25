using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using LineSharp.Globals;

namespace LineSharp.Net
{
    internal class PushClient
    {
        private readonly LineClient _client;
        private int _lsnumber;
        private HttpWebRequest _request;

        //private HttpClient httpclient;
        //X-LS number retrieved from the server

        private bool _sendFullHeaders = true;

        public PushClient(LineClient client)
        {
            _client = client;
        }

        //Send all headers?

        //Keep it as reference for when we need the access key

        public bool ReAuth
        {
            get { return _sendFullHeaders; }
            set { _sendFullHeaders = value; }
        }

        async Task<HttpWebResponse> GetResponseAsync()
        {
            var resp = await _request.GetResponseAsync();
            return (HttpWebResponse)resp;
        }

        private int count = 0;
        public byte[] CallApi(string URL, byte[] data)
        {
            Debug.Print("[PushClient] Calling an API function using a byte array of " + data.Length + " and a URL of " + URL);
            while (true)
            {
                //Create the request
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(URL);

                //It's a post request, like everything else.
                request.Method = "POST";

                //We long poll, etc in the push client so there's no timeout.
                //We should get a 410 GONE response anyways
                request.Timeout = Int32.MaxValue;

                request.UserAgent = Protocol.UserAgent;

                request.Headers.Add("X-Line-Application", Protocol.LineApplication);

                //LS Number is the internal network server number used to idenfity who we
                //are talking to inside naver's network
                if (_lsnumber != 0)
                    request.Headers.Add("X-LS", _lsnumber.ToString());

                //If we don't have it, we're assigned one.

                //Set the custom content type
                request.ContentType = "application/x-thrift";

                //If we have an access key, we must add it. Otherwise we get unauthed errors
                if (!String.IsNullOrEmpty(_client.AccessKey))
                    request.Headers.Add("X-Line-Access", _client.AccessKey);

                Stream dataStream = request.GetRequestStream();


                //Get the datastream, write it, flush, and close
                dataStream.Write(data, 0, data.Length);
                dataStream.Flush();
                dataStream.Close();
                Debug.Print("[PushClient] Sending request...");
                //Get a response.
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();

                Debug.Print("[PushClient] Acquired response from request, data length is " + response.ContentLength);

                //Get the response code, if it's not 200, log it and retry
                if (response.StatusCode != HttpStatusCode.OK)
                {
#if DEBUG
                    Console.WriteLine("Push Client recieved HTTP status code " + response.StatusCode);
                    Console.WriteLine("Content Length was " + response.ContentLength);
#endif
                    //Resend a request.
                    continue;
                }

                //At this point, only a 200 will survive.
                Debug.Print("[PushClient] Reading response bytestream...");
                byte[] resp = new byte[response.ContentLength];

                Stream responseStream = response.GetResponseStream();

                int read_count = 0;
                while (read_count < response.ContentLength)
                {
                    read_count += responseStream.Read(resp, read_count, (int)response.ContentLength - read_count);
                }
                Debug.Print("[PushClient] Finished reading, closing stream.");
                responseStream.Close();

                //We copy the byte[] response
                //Return it
                return resp;
            }
        }
    }
}