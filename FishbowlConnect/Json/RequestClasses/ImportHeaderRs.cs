using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class ImportHeaderRs :IRs
    {
        public Header Header { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get ; set ; }
    }
}
