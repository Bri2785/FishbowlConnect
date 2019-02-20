using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class ImportRs :IRs
    {
        public Row Rows { get; set; }
        public string StatusCode { get; set ; }
        public string StatusMessage { get; set ; }
    }
}
