using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

        public class FishbowlImage : NotifyOnChange
        {
            public int Id { get; set; }
            public string ImageFull { get; set; }
            public int RecordId { get; set; }
            public string TableName { get; set; }
            public string Type { get; set; }

        }
    
}
