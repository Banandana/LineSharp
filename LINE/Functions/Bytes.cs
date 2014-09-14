using System;
using System.Collections.Generic;

namespace LineSharp.Functions
{
    internal class Bytes
    {
        internal static Byte[] HexStringToByteArray(string hexString)
        {
            var bytes = new List<Byte>();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hexVal = hexString.Substring(i, 2);
                int val = Convert.ToInt32(hexVal, 16);
                bytes.Add(BitConverter.GetBytes(val)[0]);
            }
            return bytes.ToArray();
        }

        internal static byte[] GetExponentFromString(string exponent)
        {
            var exp = new byte[exponent.Length/2];
            for (int i = 0; i < exponent.Length/2; i++)
            {
                if (exponent.Substring(i*2, 2) == "01")
                {
                    exp[i] = 1;
                }
                else exp[i] = 0;
            }
            return exp;
        }

        internal static string GetHexStringFromByteArray(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }
    }
}