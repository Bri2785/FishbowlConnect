using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Exceptions
{

    /// <summary>
    /// Thrown when the request returns a status other than 1000
    /// </summary>
    public class FishbowlRequestException : FishbowlException
    {
        public FishbowlRequestException()
        {

        }
        public FishbowlRequestException(string message)
            :base(message)
        {

        }
        public FishbowlRequestException(string message, Exception inner, string statusCode = null, string innerStatusCode = null, string lastRequestRaw = null)
            : base(message,inner,statusCode, innerStatusCode, lastRequestRaw)
        {

        }
    }
}
