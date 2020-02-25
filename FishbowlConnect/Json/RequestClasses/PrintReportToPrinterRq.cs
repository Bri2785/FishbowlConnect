using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class PrintReportToPrinterRq
    {
        public int NumberOfCopies { get; set; }
        public int PrinterId { get; set; }
        public int ReportId { get; set; }
        public ParameterList ParameterList { get; set; }
    }

    public class ParameterList
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<ReportParam>))]
        public List<ReportParam> ReportParam { get; set; }
    }
}
