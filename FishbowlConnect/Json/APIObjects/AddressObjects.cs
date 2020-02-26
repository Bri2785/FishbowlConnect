using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    public partial class Address : NotifyOnChange
    {
        private string streetField;

        private string cityField;

        private string zipField;
        private State stateField;

        public string ID { get; set; }

        public string Name { get; set; }

        public string Attn { get; set; }

        public string Street
        {
            get
            {
                return this.streetField;
            }
            set
            {
                this.streetField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string Zip
        {
            get
            {
                return this.zipField;
            }
            set
            {
                this.zipField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string LocationGroupID { get; set; }

        /// <remarks/>
        public bool Default { get; set; }

        /// <remarks/>
        public string Residential { get; set; }

        /// <remarks/>
        [JsonConverter(typeof(AddressTypeEnumToStringConverter))]
        public AddressType Type { get; set; }

        /// <remarks/>
        public State State
        {
            get
            {
                return this.stateField;
            }
            set
            {
                this.stateField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public Country Country { get; set; }

        public AddressInformationList AddressInformationList { get; set; }
    }



    public partial class State
    {


        public int ID { get; set; }

        /// <remarks/>
        public string Code { get; set; }

        /// <remarks/>
         //[JsonIgnore]
        public string Name { get; set; }

        //[JsonIgnore]
        public int CountryID { get; set; }
    }


    public partial class Country
    {

        /// <remarks/>
        public int ID { get; set; }

        //[JsonIgnore]
        public string Code { get; set; }
        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        
    }

    public partial class Addresses : NotifyOnChange
    {

        private ObservableCollection<Address> addressField;

        public ObservableCollection<Address> Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
                RaisePropertyChanged();
            }
        }
    }



    public partial class ShipmentAddress : NotifyOnChange
    {
        private string streetField;

        private string cityField;

        private string zipField;
        private ShipmentState stateField;

        public string ID { get; set; }

        public string Name { get; set; }

        public string Attn { get; set; }

        public string Street
        {
            get
            {
                return this.streetField;
            }
            set
            {
                this.streetField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string Zip
        {
            get
            {
                return this.zipField;
            }
            set
            {
                this.zipField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string LocationGroupID { get; set; }

        /// <remarks/>
        public bool Default { get; set; }

        /// <remarks/>
        public string Residential { get; set; }

        /// <remarks/>
        [JsonConverter(typeof(AddressTypeEnumToStringConverter))]
        public AddressType Type { get; set; }

        /// <remarks/>
        public ShipmentState State
        {
            get
            {
                return this.stateField;
            }
            set
            {
                this.stateField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public ShipmentCountry Country { get; set; }

        //public AddressInformationList AddressInformationList { get; set; }
    }



    public partial class ShipmentState
    {


        public int ID { get; set; }

        /// <remarks/>
        public string Code { get; set; }

        /// <remarks/>
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public int CountryID { get; set; }
    }


    public partial class ShipmentCountry
    {

        /// <remarks/>
        public int ID { get; set; }

        [JsonIgnore]
        public string Code { get; set; }
        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>

    }

}
