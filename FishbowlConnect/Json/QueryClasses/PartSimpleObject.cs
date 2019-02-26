using FishbowlConnect.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    public class PartSimpleObject : IPPSimple
    {

        public string Number { get; set; }
        public string Description { get; set; }
    }
}
