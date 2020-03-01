using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    public class Report
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }

        public string ReportDescription { get; set; }

    }


    public partial class ReportParam
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ReportParam()
        {

        }
        public ReportParam(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name + " - " + Value;
        }
    }

    public partial class Printers
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<Object>))]
        public List<Printer> Printer { get; set; }
    }

    public class Printer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public Computer Computer { get; set; }

    }

    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }

}
