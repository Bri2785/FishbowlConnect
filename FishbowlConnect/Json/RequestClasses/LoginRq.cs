using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class LoginRq
    {
        public string IAID { get; set; }
        public string IADescription { get; set; }
        public string UserName { get; set; }
        public string IAName { get; set; }
        public string UserPassword { get; set; }
    }
}
