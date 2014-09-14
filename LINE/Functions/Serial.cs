using System.IO;
using System.Reflection;
using LineSharp.Datatypes;
using Thrift.Protocol;
using Thrift.Transport;

namespace LineSharp.Functions
{
    internal class Serial
    {
        //This shit only exists because the internal LINE client transport doesn't handle
        //more than a single connection at a time, so if it was being used for long polling,
        //we'd be fucked because it would probably freeze the entire thing and prevent any
        //other calls from happening. It's not something I'm proud to have written but it
        //works wonders if you need to call something in parallel. It's just expensive. :O

        internal static byte[] Serialize(string clientFunction, object[] parameters)
        {
            var serialstream = new MemoryStream(4096);
            TTransport transport = new TStreamTransport(serialstream, serialstream);
            transport.Open();
            TProtocol protocol = new TCompactProtocol(transport);
            var client = new TalkService.Client(protocol);

            //.MakeGenericMethod(
            //hook.Invoke(Client, parameters);
            MethodInfo callingFunction = client.GetType().GetMethod(clientFunction);

            callingFunction.Invoke(client, parameters);

            byte[] data = serialstream.ToArray();
            //MemoryStream serialstream = new MemoryStream(4096);
            //TTransport transport = new TStreamTransport(serialstream, serialstream); transport.Open();


            return data;
        }

        internal static byte[] SerializeOperation(Operation O)
        {
            var serialstream = new MemoryStream(4096);
            TTransport transport = new TStreamTransport(serialstream, serialstream);
            transport.Open();
            TProtocol protocol = new TCompactProtocol(transport);
            var client = new TalkService.Client(protocol);

            //.MakeGenericMethod(
            //hook.Invoke(Client, parameters);

            O.Write(protocol);


            byte[] data = serialstream.ToArray();
            //MemoryStream serialstream = new MemoryStream(4096);
            //TTransport transport = new TStreamTransport(serialstream, serialstream); transport.Open();


            return data;
        }

        internal static object Deserialize(string clientFunction, byte[] data)
        {
            var serialstream = new MemoryStream(data);
            TTransport transport = new TStreamTransport(serialstream, serialstream);
            transport.Open();
            TProtocol protocol = new TCompactProtocol(transport);
            var client = new TalkService.Client(protocol);
            MethodInfo callingFunction = client.GetType().GetMethod(clientFunction);

            //Magic to redirect the possible exception to the end user's code
            //or at least the nearest TalkException catch
            try
            {
                return callingFunction.Invoke(client, null);
            }
            catch (TargetInvocationException E)
            {
                if (E.InnerException is TalkException)
                {
                    throw E.InnerException;
                }
            }
            return null;
        }
    }
}