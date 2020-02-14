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
    }
}
