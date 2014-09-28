using System;
using System.Collections.Generic;
using System.Threading;
using LineSharp.Datatypes;
using LineSharp.Functions;
using LineSharp.Globals;
using LineSharp.Net;

namespace LineSharp
{
    internal class OperationHandler
    {
        private readonly LineClient _client;
        private readonly PushClient connection;
        private Thread listeningThread;

        public OperationHandler(LineClient Client)
        {
            Debug.Print("Creating an operation handler, Client access key given is " + Client.AccessKey);
            connection = new PushClient(Client);
            _client = Client;
        }

        //Fucking catch a talkException for god's sake
        //public long GetOpRevision()
        //{
        //    Debug.Print("[OpHandler] Getting op-revision.");
        //    try
        //    {
        //        long oprev = (long)service.getLastOpRevision();
        //        Debug.Print("[OpHandler] Got oprev of " + oprev);
        //        return oprev;
        //    }
        //    catch (Exception E)
        //    {
        //        Debug.Print("[OpHandler] Failed to get oprev. What happened?");
        //        Debug.Print(E.Message);
        //        Debug.Print(E.Source);
        //        Debug.Print(E.StackTrace);
        //        if (E.InnerException is TalkException)
        //        {
        //            throw E.InnerException;
        //            //Take it up a notch, son
        //        }
        //        return 0;
        //    }
        //}

        public void Start()
        {
            Debug.Print("[OpHandler] Starting the listening thread.");
            //Listen();
            if (listeningThread == null)
            {
                listeningThread = new Thread(Listen);
                listeningThread.Start();
            }
        }


        public void Stop()
        {
            Debug.Print("[OpHandler] Stopping the listening thread.");
            if (listeningThread != null)
            {
                listeningThread.Abort();
                listeningThread = null;
            }
        }

        private void Listen()
        {
            //This originally used the line transport, which is NOT BUILT for long polling.

            Debug.Print("[OpHandler] Listen() is getting the oprev.");

            //Get the byteresponse when fetching the oprevision
            byte[] oprev_response = connection.CallApi(URL.TalkService, Serial.Serialize("send_getLastOpRevision", new object[] { }));
            //Get a long value from serializing it

            //It looks dirty but it simply gets the byte[] array from calling a function
            //This honestly could be done better but at the same time, it's work.
            long oprev = (long)Serial.Deserialize("recv_getLastOpRevision", oprev_response);

            Debug.Print("[OpHandler] Beginning listener loop.");

            while (true)
            {
                Debug.Print("[OpHandler] Fetching operations with oprev=" + oprev);

                byte[] fetch_operations_response = connection.CallApi(URL.P, Serial.Serialize("send_fetchOperations", new object[] { oprev, 50 }));
                
                List<Operation> fetch = (List<Operation>)Serial.Deserialize("recv_fetchOperations", fetch_operations_response);
                
                //List<Operation> fetch = service.fetchOperations(oprev, 50);
                Debug.Print("[OpHandler] Retrieved operation list of " + fetch.Count);
                for (int i = 0; i < fetch.Count; i++)
                {
                    if (fetch[i].Type != OpType.END_OF_OPERATION)
                    {
                        oprev = fetch[i].Revision;
                        //Register the operation recieved to the LineClient.

                        if (_client != null)
                        {
                            Debug.Print("[OpHandler] Dispatching operations.");
                            _client.RegisterOperation(fetch[i]);
                        }
                        else
                            Debug.Print("[OpHandler] Client is null, cannot dispatch operation.");
                    }
                }
            }
        }
    }
}