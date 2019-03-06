using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class CustomField
    {

        /// <remarks/>
        public string ID { get; set; }

        /// <remarks/>
        public string Type { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Description { get; set; }

        /// <remarks/>
        public string SortOrder { get; set; }

        /// <remarks/>
        public string Info { get; set; }

        /// <remarks/>
        public string RequiredFlag { get; set; }

        /// <remarks/>
        public string ActiveFlag { get; set; }

        /// <remarks/>
        public CustomList CustomList { get; set; }
    }


    public partial class CustomList
    {

        /// <remarks/>
        public string ID { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Description { get; set; }

        /// <remarks/>

        [JsonConverter(typeof(ListOrSingleValueConverter<CustomListItem>))]
        public List<CustomListItem> CustomListItems { get; set; }
    }


    public partial class CustomListItem
    {

        /// <remarks/>
        public string ID { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Description { get; set; }
    }

   
    public partial class CustomFields
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<CustomField>))]
        public List<CustomField> CustomField { get; set; }
    }
}
