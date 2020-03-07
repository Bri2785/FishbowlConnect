using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Helpers
{
    public static class StringExtensions
    {
        public static string EscapeForMySQL(this string input)
        {
            return input.Replace("_", "\\_");
        }

        public static byte[] CreateMD5(this string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                //byte[] inputBytes = Encoding.GetEncoding(28591).GetBytes(input); //same in Java

                byte[] inputBytes = Encoding.Default.GetBytes(input); //same in Java

                //string inputBase64 = Convert.ToBase64String(inputBytes);

                byte[] hashBytes = md5.ComputeHash(inputBytes); //java uses signed, and unsigned values are different

                //sbyte[] signed = Array.ConvertAll(hashBytes, b => unchecked((sbyte)b));
                //return Convert.ToBase64String(hashBytes);
                return hashBytes;


                //// Convert the byte array to hexadecimal string
                //StringBuilder sb = new StringBuilder();
                //for (int i = 0; i < hashBytes.Length; i++)
                //{
                //    sb.Append(hashBytes[i].ToString("X2"));
                //}
                //return sb.ToString();
            }
        }
    }
}
