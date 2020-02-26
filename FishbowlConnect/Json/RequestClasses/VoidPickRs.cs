using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.RequestClasses
{
    /// <summary>
    /// Response Object for the VoidPick Rq
    /// </summary>
    public class VoidPickRs : IRs
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public Pick Pick { get; set; }
        public string UnvoidableItems { get; set; }
    }
}
