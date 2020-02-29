using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class PrintReportToPrinterRs : IRs
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public int JobId { get; set; }
    }
}
