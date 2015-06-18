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

        //Thanks to Evance for fixing this function
        internal static byte[] GetExponentFromString(string exponent)
        {
            double ExponentLen_Double = Convert.ToDouble(exponent.Length) / 2;
            int ExponentByteCount = Convert.ToInt16(Math.Ceiling(ExponentLen_Double));
            if (ExponentByteCount != exponent.Length)
            {
                exponent = "0" + exponent;
            }

            byte[] exp = new byte[ExponentByteCount];
            for (int i = 0; i < ExponentByteCount; i++)
            {
                if (exponent.Substring(i * 2, 2) == "01")
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