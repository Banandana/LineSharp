using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using LineSharp.Globals;
using Thrift.Transport;

namespace LineSharp.Net
{
    internal class LineTransport : TTransport
    {
        #region Keys and Stuff

        private readonly string _lineApplication;
        private readonly string _userAgent;

        private MemoryStream _readStream;
        private HttpWebRequest _request;
        private byte[] _response;
        private MemoryStream _writeStream;

        public string AccessKey { get; set; }

        public string TargetUrl { get; set; }

        #endregion

        private bool _isOpen;
        private int _lsnumber;

        public LineTransport(string userAgent, string application)
        {
            _userAgent = userAgent;
            _lineApplication = application;
        }

        public override bool IsOpen
        {
            get { return _isOpen; }
        }

        public override void Open()
        {
            Console.WriteLine("Opening transport");
            _isOpen = true;
        }

        public override void Close()
        {
            Console.WriteLine("Closing transport");
            _isOpen = false;

        }

        protected override void Dispose(bool disp)
        {

        }

        async Task<HttpWebResponse> GetResponseAsync()
        {
            var resp = await _request.GetResponseAsync();
            return (HttpWebResponse)resp;
        }

        public override void Flush()
        {
            base.Flush();

            //Only send and ask for a reply if there
            //is actually data that needs to be sent
            if (_writeStream.Length > 0)
            {
                //Dump the writestream into an array of bytes to be sent
                byte[] data = _writeStream.ToArray();

                //Create the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TargetUrl);

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
                if (!String.IsNullOrEmpty(AccessKey))
                    request.Headers.Add("X-Line-Access", AccessKey);

                Stream dataStream = request.GetRequestStream();


                //Get the datastream, write it, flush, and close
                dataStream.Write(data, 0, data.Length);
                dataStream.Flush();
                dataStream.Close();

                //Get a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Get the response code, if it's not 200, log it and retry
                if (response.StatusCode != HttpStatusCode.OK)
                {
#if DEBUG
                    Console.WriteLine("Push Client recieved HTTP status code " + response.StatusCode);
                    Console.WriteLine("Content Length was " + response.ContentLength);
#endif
                }

                byte[] resp = new byte[response.ContentLength];

                Stream responseStream = response.GetResponseStream();

                int read_count = 0;
                while (read_count < response.ContentLength)
                {
                    read_count += responseStream.Read(resp, read_count, (int)response.ContentLength - read_count);
                }

                responseStream.Close();

                _readStream = new MemoryStream(resp);
                _writeStream = new MemoryStream();
            }
        }

        public override void Write(byte[] buf, int off, int len)
        {
            if (_writeStream == null || !_writeStream.CanWrite)
            {
                _writeStream = new MemoryStream();
            }

            _writeStream.Write(buf, off, len);
        }

        public override int Read(byte[] buf, int off, int len)
        {
            if (_readStream == null || !_readStream.CanWrite)
            {
                _readStream = new MemoryStream(_response);
            }
            return _readStream.Read(buf, off, len);
        }
    }
}