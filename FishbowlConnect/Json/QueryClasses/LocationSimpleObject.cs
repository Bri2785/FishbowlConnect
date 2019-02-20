using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    public class LocationSimpleObject
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string LGName { get; set; }
        public int LocationGroupId { get; set; }
        public int LocationTagNum { get; set; }


        public string LocationFullName { get { return LGName + "-" + LocationName; } }
    }
}
