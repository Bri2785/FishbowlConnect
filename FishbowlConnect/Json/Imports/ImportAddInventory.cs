using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Imports
{
    public class ImportAddInventory
    {
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string Location { get; set; }
        public int Qty { get; set; }
        public string UOM { get; set; }
        public string Cost { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }

        //There are also required tracking fields but they are dynamic and are set in the method manually
    }
}
