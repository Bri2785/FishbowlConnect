using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    public class PartDefaultLocationObject
    {
        public int LocationGroupId { get; set; }
        public string LocationGroupName { get; set; }
        public int PartId { get; set; }
        public string PartNum { get; set; }        
        public int LocationId { get; set; }

        [Default(null)]
        public string LocationName { get; set; }

    }
}
