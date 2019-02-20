using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Imports
{
    public class ImportCycleCountData
    {
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string Location { get; set; }
        public int Qty { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
        public string Customer { get; set; }
        
        //There are also required tracking fields but they are dynamic and are set in the method manually
    }
}
