using System.IO;
using System.Text;

namespace LineSharp.Functions
{
    internal class Auth
    {
        internal static byte[] GenerateAuthCode(string sessionId, string username, string password)
        {
            var data = new MemoryStream(sessionId.Length + username.Length + password.Length + 3);

            data.Write(new[] {(byte) sessionId.Length}, 0, 1);
            data.Write(Encoding.ASCII.GetBytes(sessionId.ToCharArray()), 0, sessionId.Length);

            data.Write(new[] {(byte) username.Length}, 0, 1);
            data.Write(Encoding.ASCII.GetBytes(username.ToCharArray()), 0, username.Length);

            data.Write(new[] {(byte) password.Length}, 0, 1);
            data.Write(Encoding.ASCII.GetBytes(password.ToCharArray()), 0, password.Length);

            return data.ToArray();
        }
    }
}