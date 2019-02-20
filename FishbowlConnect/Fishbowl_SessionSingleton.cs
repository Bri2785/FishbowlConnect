using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public sealed class Fishbowl_SessionSingleton
    {
        private static readonly FishbowlSession _FBSession = new FishbowlSession("192.168.150.4");
        //private static string _ipAddress = "192.168.150.4";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Fishbowl_SessionSingleton()
        {
        }

        private Fishbowl_SessionSingleton()
        {
            
        }
        

        public static FishbowlSession Instance
        {
            get
            {
                return _FBSession;
            }
        }
    }
}
