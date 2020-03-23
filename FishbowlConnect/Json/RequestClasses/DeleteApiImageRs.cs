using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.RequestClasses
{
    public class DeleteApiImageRs : IRs
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
