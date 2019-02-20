using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Exceptions
{
    /// <summary>
    /// Thrown when there's a problem with logging in
    /// </summary>
    public class FishbowlAuthException : FishbowlException
    {
        public FishbowlAuthException()
        {

        }
        public FishbowlAuthException(string message):
            base(message)
        {

        }

        public FishbowlAuthException(string message, Exception inner, string statusCode = null, string innerStatusCode = null)
            : base(message, inner, statusCode, innerStatusCode)
        {

        }
    }
}
