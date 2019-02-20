using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Exceptions
{
    public class FishbowlException : Exception
    {
        public string StatusCode { get; set; }
        public string InnerStatusCode { get; set; }
        public string LastRequestRaw { get; set; }

        public FishbowlException()
        {

        }

        public FishbowlException(string message)
            :base(message)
        {

        }

        /// <summary>
        /// Fishbowl Exception
        /// </summary>
        /// <param name="message">Fishbowl Error Message</param>
        /// <param name="statusCode">Fishbowl Status Code</param>
        /// <param name="innerStatusCode">Fishbowl Inner Response Status Code</param>
        /// <param name="inner">Inner Exception</param>
        public FishbowlException(string message, Exception inner, string statusCode = null, string innerStatusCode = null, string lastRequestRaw = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            InnerStatusCode = innerStatusCode;
            LastRequestRaw = lastRequestRaw;
        }
    }
}
