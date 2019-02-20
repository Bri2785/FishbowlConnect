using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Exceptions
{
    /// <summary>
    /// Thrown when client cannot connect to the server socket
    /// </summary>
    public class FishbowlConnectionException : FishbowlException
    {
        public FishbowlConnectionException()
        {

        }

        public FishbowlConnectionException(string message)
            : base(message)
        {

        }

        public FishbowlConnectionException(string message, Exception inner, string statusCode = null, string innerStatusCode = null)
            : base(message, inner, statusCode, innerStatusCode)
        {

        }
    }
}
