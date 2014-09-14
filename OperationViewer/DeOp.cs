using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

using LineSharp.Datatypes;

namespace OperationViewer
{
    class DeOp
    {
        internal static object Deserialize(string ClientFunction, byte[] data)
        {
            MemoryStream serialstream = new MemoryStream(data);
            TTransport transport = new TStreamTransport(serialstream, serialstream); transport.Open();
            TProtocol protocol = new TCompactProtocol(transport);
            TalkService.Client Client = new TalkService.Client(protocol);
            MethodInfo CallingFunction = Client.GetType().GetMethod(ClientFunction);

            try
            {
                return CallingFunction.Invoke(Client, null);
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
