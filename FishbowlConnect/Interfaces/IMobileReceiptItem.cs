using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Interfaces
{
    public interface IMobileReceiptItem
    {
        int id { get; set; }
        int mrId { get; set; }
        string upc { get; set; }
        DateTime timeScanned { get; set; }
        int statusID { get; set; }
    }
}
