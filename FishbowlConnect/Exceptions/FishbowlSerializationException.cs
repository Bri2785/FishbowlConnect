using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Exceptions
{
    /// <summary>
    /// Thrown on serialization error
    /// </summary>
    public class FishbowlSerializationException: FishbowlException
    {

        public FishbowlSerializationException()
        {

        }
        public FishbowlSerializationException(string message) :
            base(message)
        {

        }

        public FishbowlSerializationException(string message, Exception inner, string statusCode = null, string innerStatusCode = null)
            : base(message, inner, statusCode, innerStatusCode)
        {

        }
    }
}
