using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Interfaces
{
    public interface IMobileReceipt
    {
        
        int mrId { get; set; }
        string description { get; set; }
        DateTime timeStarted { get; set; }
        DateTime timeFinished { get; set; }
        DateTime timeUploaded { get; set; }
        int statusId { get; set; }
    }
}
