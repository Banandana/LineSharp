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
        private readonly LineClient client;
        TalkService.Client service;
        LineTransport transport;
        private readonly PushClient connection;
        private Thread listeningThread;

        public OperationHandler(LineClient Client)
        {
            connection = new PushClient(Client);
            client = Client;
        }

        //Fucking catch a talkException for god's sake
        public long GetOpRevision()
        {
            try
            {
                return (long)service.getLastOpRevision();
            }
            catch (Exception E)
            {
                if (E.InnerException is TalkException)
                {
                    throw E.InnerException;
                    //Take it up a notch, son
                }
                return 0;
            }
        }

        public void Start()
        {
            //Listen();
            if (listeningThread == null)
            {
                listeningThread = new Thread(Listen);
                listeningThread.Start();
            }
        }


        public void Stop()
        {
            if (listeningThread != null)
            {
                listeningThread.Abort();
                listeningThread = null;
            }
        }

        private void Listen()
        {
            transport = new LineTransport(Protocol.UserAgent, Protocol.LineApplication);
            transport.AccessKey = client.AccessKey;
            transport.TargetUrl = URL.P;
            service = new TalkService.Client(new Thrift.Protocol.TCompactProtocol(transport));
            long oprev = GetOpRevision();
            while (true)
            {
                List<Operation> fetch = service.fetchOperations(oprev, 50);
                for (int i = 0; i < fetch.Count; i++)
                {
                    if (fetch[i].Type != OpType.END_OF_OPERATION)
                    {
                        oprev = fetch[i].Revision;
                        //Register the operation recieved to the LineClient.

                        if (client != null)
                        {
                            client.RegisterOperation(fetch[i]);
                        }
                    }
                }
            }
        }
    }
}