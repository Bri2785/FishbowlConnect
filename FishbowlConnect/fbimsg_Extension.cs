using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using FishbowlConnect.Interfaces;
using Newtonsoft.Json;

/// <remarks/>
namespace FishbowlConnect
{
    [System.Serializable]
    public class NotifyOnChange : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    //public partial class AddressInformation
    //{
    //    private string ContactTypeNameField;

    //    [XmlIgnore]
    //    public string ContactTypeName
    //    {
    //        get { return this.ContactTypeNameField; }
    //        set
    //        {
    //            ContactTypeNameField = value;
    //            RaisePropertyChanged();
    //        }
    //    }

    //}
    public partial class SalesOrder
    {
        private int EDITxnIDField;

        private string MagIncrementIDField;

        private uint MagEntityIDField;

        private string email;

        private string phone;

        [XmlIgnore]
        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = value;
            }
        }

        [XmlIgnore]
        public string Phone
        {
            get
            {
                return this.phone;
            }
            set
            {
                this.phone = value;
            }
        }

        [XmlIgnore]
        public int EDITxnID
        {
            get
            {
                return this.EDITxnIDField;
            }
            set
            {
                this.EDITxnIDField = value;
            }
        }
        [XmlIgnore]
        public string MagIncrementID
        {
            get
            {
                return this.MagIncrementIDField;
            }
            set
            {
                this.MagIncrementIDField = value;
            }
        }
        [XmlIgnore]
        public uint MagEntityID
        {
            get
            {
                return this.MagEntityIDField;
            }
            set
            {
                this.MagEntityIDField = value;
            }
        }
    }

    public partial class Customer : NotifyOnChange
    {
        private string customerGroupField;

        private uint magentoCustIDField;

        [XmlIgnore]
        public string customerGroup
        {
            get
            {
                return this.customerGroupField;
            }
            set
            {
                this.customerGroupField = value;
                RaisePropertyChanged();

            }
        }

        [XmlIgnore]
        public uint magentoCustID
        {
            get
            {
                return this.magentoCustIDField;
            }
            set
            {
                this.magentoCustIDField = value;
            }
        }
    }

    public enum AddressType
    {

        /// <remarks/>

        Home,

        /// <remarks/>
        //[XmlEnum(Name = "Main Office")]
        Main,

        /// <remarks/>
        //[XmlEnum(Name = "Ship To")]
        Ship,

        /// <remarks/>
        //[XmlEnum(Name = "Bill To")]
        Bill,

        /// <remarks/>
        //[XmlEnum(Name = "Remit To")]
        Remit,
    }

    //deprecated from old REEL version
    //public partial class InvQty : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    public void RaisePropertyChanged([CallerMemberName] string property = null)
    //    {
    //        var propChanged = PropertyChanged;
    //        if (propChanged != null)
    //        {
    //            propChanged(this, new PropertyChangedEventArgs(property));
    //        }
    //    }

    //    private bool _isSelected;

    //    [XmlIgnore]
    //    public string InventorySummary
    //    {
    //        get
    //        {
    //            return locationField.LocationGroupName + " - " + locationField.Name +
    //                " - " + qtyOnHandField;
    //        }
    //    }
    //    [XmlIgnore]
    //    public string FullLocation
    //    {
    //        get
    //        {
    //            return locationField.LocationGroupName + " - " + locationField.Name;
    //        }
    //    }
    //    [XmlIgnore]
    //    public bool IsSelected
    //    {
    //        get { return _isSelected; }
    //        set
    //        {
    //            _isSelected = value;
    //            RaisePropertyChanged();
    //        }
    //    }

    //}

    public partial class Location1 : NotifyOnChange
    {
        [XmlIgnore]
        public string FullLocation
        {
            get
            {
                return LocationGroupName + " - " + Name;
            }
        }

        private bool _isSelected;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }
    }

    public partial class TrackingItem
    {

        /// <remarks/>
        [XmlIgnore]
        public string TrackingSummary
        {
            get
            {
                if (PartTracking.TrackingTypeID == "20" || PartTracking.TrackingTypeID == "30")
                {
                    if (TrackingValue != null)
                    {
                        return PartTracking.Abbr + " - " + Convert.ToDateTime(TrackingValue).ToString("d");
                    }

                }

                return PartTracking.Abbr + " - " + TrackingValue;


            }

        }
    }

    public class PartDefaultLocationSimple : INotifyPropertyChanged
    {
        private Location1 _currentDefaultLocation;

        public event PropertyChangedEventHandler PropertyChanged;


        public Part PartToSet { get; set; }
        public string LocationGroupID { get; set; }
        public string LocationGroupName { get; set; }
        public string LocationTagNumber { get; set; }

        public Location1 CurrentDefaultLocation
        {
            get
            {
                return _currentDefaultLocation;
            }
            set
            {
                _currentDefaultLocation = value;
                PropertyChanged?.Invoke(this,
    new PropertyChangedEventArgs("currentDefaultLocationSummary"));
            }
        }

        public string currentDefaultLocationSummary
        {
            get
            {
                if (CurrentDefaultLocation != null)
                {
                    return LocationGroupName + " - " + CurrentDefaultLocation.Name;
                }
                else
                {
                    return LocationGroupName + " - (not set)";
                }
            }
        }
    }

    //public partial class Pick : NotifyOnChange
    //{
    //    [XmlIgnore]
    //    public string OrderInfo
    //    {
    //        get
    //        {
    //            if (PickOrders.Count > 0)
    //            {
    //                return PickOrders[0].OrderTo;
    //            }
    //            return "Unknown";
    //        }
    //    }

    //    [XmlIgnore]
    //    public string DateScheduledFormatted
    //    {
    //        get
    //        {
    //            return Convert.ToDateTime(DateScheduled).ToShortDateString();
    //        }
    //    }
    //}

    //public partial class PickItem : NotifyOnChange
    //{

        

    //}

    public enum StateEnum
    {
        [Description("Alabama")]
        AL,

        [Description("Alaska")]
        AK,

        [Description("Arkansas")]
        AR,

        [Description("Arizona")]
        AZ,

        [Description("California")]
        CA,

        [Description("Colorado")]
        CO,

        [Description("Connecticut")]
        CT,

        [Description("D.C.")]
        DC,

        [Description("Delaware")]
        DE,

        [Description("Florida")]
        FL,

        [Description("Georgia")]
        GA,

        [Description("Hawaii")]
        HI,

        [Description("Iowa")]
        IA,

        [Description("Idaho")]
        ID,

        [Description("Illinois")]
        IL,

        [Description("Indiana")]
        IN,

        [Description("Kansas")]
        KS,

        [Description("Kentucky")]
        KY,

        [Description("Louisiana")]
        LA,

        [Description("Massachusetts")]
        MA,

        [Description("Maryland")]
        MD,

        [Description("Maine")]
        ME,

        [Description("Michigan")]
        MI,

        [Description("Minnesota")]
        MN,

        [Description("Missouri")]
        MO,

        [Description("Mississippi")]
        MS,

        [Description("Montana")]
        MT,

        [Description("North Carolina")]
        NC,

        [Description("North Dakota")]
        ND,

        [Description("Nebraska")]
        NE,

        [Description("New Hampshire")]
        NH,

        [Description("New Jersey")]
        NJ,

        [Description("New Mexico")]
        NM,

        [Description("Nevada")]
        NV,

        [Description("New York")]
        NY,

        [Description("Oklahoma")]
        OK,

        [Description("Ohio")]
        OH,

        [Description("Oregon")]
        OR,

        [Description("Pennsylvania")]
        PA,

        [Description("Rhode Island")]
        RI,

        [Description("South Carolina")]
        SC,

        [Description("South Dakota")]
        SD,

        [Description("Tennessee")]
        TN,

        [Description("Texas")]
        TX,

        [Description("Utah")]
        UT,

        [Description("Virginia")]
        VA,

        [Description("Vermont")]
        VT,

        [Description("Washington")]
        WA,

        [Description("Wisconsin")]
        WI,

        [Description("West Virginia")]
        WV,

        [Description("Wyoming")]
        WY

    }

    public enum CustomerGroups
    {
        Retail,
        Distributor


    }

    public enum CarrierEnum
    {
        [Description("Pickup - Main")]
        PickupMain,

        [Description("FedEx-Parcel-Ground")]
        FedExParcelGround,

        [Description("Delivery")]
        Delivery,

        [Description("FedEx-Air-2nd Day Air")]
        FedExAir2ndDayAir,

        [Description("FedEx-Air-2nd Day AM")]
        FedExAir2ndDayAM,

        [Description("FedEx-Air-Express Saver")]
        FedExAirExpressSaver,

        [Description("Fedex-Air-First Overnight")]
        FedexAirFirstOvernight,

        [Description("FedEx-Air-Priority Overnight")]
        FedExAirPriorityOvernight,

        [Description("FedEx-Air-Standard Overnight")]
        FedExAirStandardOvernight,

        [Description("FedEx-Freight")]
        FedExFreight,

        [Description("FedEx-Parcel-FDX Home Delivery")]
        FedExParcelFDXHomeDelivery,

        [Description("FedEx-Air-2nd Day Air")]
        LocalInstall,


        [Description("Conway")]
        Conway

    } //TODO: change to match new ship carrier and service fields

    public partial class ProductAvailableInventory: NotifyOnChange
    {
        private string availableForSale;

        public string AvailableForSale
        {
            get { return availableForSale; }
            set
            {
                availableForSale = value;
                RaisePropertyChanged();
            }
        }

        private string availableToBuild;

        public string AvailableToBuild
        {
            get { return availableToBuild; }
            set
            {
                availableToBuild = value;
                RaisePropertyChanged();
            }
        }

        private string availableInStock;

        public string AvailableInStock
        {
            get { return availableInStock; }
            set { availableInStock = value;
                RaisePropertyChanged();
            }
        }



    }

    public partial class ProductSpec : NotifyOnChange
    {
        private string specName;

        public string SpecName
        {
            get { return specName; }
            set
            {
                specName = value;
                RaisePropertyChanged();
            }
        }

        private string specValue;

        public string SpecValue
        {
            get { return specValue; }
            set
            {
                specValue = value;
                RaisePropertyChanged();
            }
        }

    }

    public partial class ProductSimple : NotifyOnChange
    {
        private string productNumber;

        public string ProductNumber
        {
            get { return productNumber; }
            set
            {
                productNumber = value;
                RaisePropertyChanged();
            }
        }
        private string productDescription;

        public string ProductDescription
        {
            get { return productDescription; }
            set
            {
                productDescription = value;
                RaisePropertyChanged();
            }
        }

        private string productTree;

        public string ProductTree
        {
            get { return productTree; }
            set
            {
                productTree = value;
                RaisePropertyChanged();
            }
        }

        private decimal price;

        public decimal Price
        {
            get { return price; }
            set
            {
                price = value;
                RaisePropertyChanged();
            }
        }

        private string upc;

        public string UPC
        {
            get { return upc; }
            set
            {
                upc = value;
                RaisePropertyChanged();
            }
        }

        private int id;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged();
            }
        }


    }

    public partial class MobileReceipt : NotifyOnChange, IMobileReceipt
    {
        public int id { get; set; }
        public int mrId { get; set; }
        public string description { get; set; }
        public DateTime timeStarted { get; set; }
        public DateTime timeFinished { get; set; }
        public DateTime timeUploaded { get; set; }
        public int statusId { get; set; }
    }

    public partial class MobileReceiptItem : NotifyOnChange, IMobileReceiptItem
    {
        public int id { get; set; }
        public int mrId { get; set; }
        public string upc { get; set; }
        public DateTime timeScanned { get; set; }
        public int statusID { get; set; }

    }

}