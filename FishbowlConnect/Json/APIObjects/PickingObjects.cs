using FishbowlConnect.Helpers;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    public partial class Pick : NotifyOnChange
    {
        private PickItems pickItemsField;

        public string PickID { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string TypeID { get; set; }
        public string Status { get; set; }
        public string StatusID { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string LocationGroupID { get; set; }
        public DateTime DateLastModified { get; set; }
        public DateTime DateScheduled { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateFinished { get; set; }
        public string UserName { get; set; }

        public PickOrders PickOrders { get; set; }

        
        public PickItems PickItems
        {
            get
            {
                return this.pickItemsField;
            }
            set
            {
                this.pickItemsField = value;
                RaisePropertyChanged();
            }
        }
        public string OrderInfo
        {
            get
            {
                //if (PickOrders?.Count > 0)
                //{
                //    return PickOrders?[0].OrderTo;
                //}
                return PickOrders?.PickOrder?.OrderTo;
                //return "Unknown";
            }
        }
        public string DateScheduledFormatted
        {
            get
            {
                return DateScheduled.ToShortDateString();
                //return Convert.ToDateTime(DateScheduled).ToShortDateString();
            }
        }
    }

    public partial class PickOrders : NotifyOnChange
    {
        public PickOrder PickOrder { get; set; }
    }

    public partial class PickOrder : NotifyOnChange
    {

        public string OrderType { get; set; }

        public string OrderTypeID { get; set; }

        public string OrderNum { get; set; }

        public string OrderID { get; set; }

        public string OrderTo { get; set; }

        public string Note { get; set; }
    }

    [Serializable()]
    public class PickItem : NotifyOnChange
    {
        public string PickItemID { get; set; }
        private string statusField;
        private Part partField;
        private decimal quantityField;
        private Tracking trackingField;
        private TagType destinationTagField;
        private Location1 locationField;
        
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
                RaisePropertyChanged();
            }
        }

        public Part Part
        {
            get
            {
                return this.partField;
            }
            set
            {
                this.partField = value;
                RaisePropertyChanged();
            }
        }

        public Tag Tag { get; set; }

        public decimal Quantity
        {
            get
            {
                return this.quantityField;
            }
            set
            {
                this.quantityField = value;
                RaisePropertyChanged();
            }
        }

        public UOM UOM { get; set; }

        public Tracking Tracking
        {
            get
            {
                return this.trackingField;
            }
            set
            {
                this.trackingField = value;
                RaisePropertyChanged();
            }
        }

        public TagType DestinationTag
        {
            get
            {
                return this.destinationTagField;
            }
            set
            {
                this.destinationTagField = value;
                RaisePropertyChanged();
            }
        }

        public string OrderType { get; set; }

        public string OrderTypeID { get; set; }

        public string OrderNum { get; set; }

        public string OrderID { get; set; }

        public string SoItemId { get; set; }

        public string PoItemId { get; set; }

        public string XoItemId { get; set; }

        public string WoItemId { get; set; }

        public string SlotNumber { get; set; }

        public string Note { get; set; }

        //public string SourceTagID { get; set; }

        public Location1 Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ItemStartedLocation));
            }
        }

        public string PickItemType { get; set; }

        [JsonIgnore]
        public string ItemStartedLocation
        {
            get
            {
                if (Location != null)
                {
                    return Location.Name;
                }
                return "No Location";
            }
        }
    }

    public partial class PickItems: NotifyOnChange
    {
        private FullyObservableCollection<PickItem> pickItems;

        [JsonConverter(typeof(FullyObservableCollectionOrSingleValueConverter<PickItem>))]
        public FullyObservableCollection<PickItem> PickItem
        {
            get
            {
                return this.pickItems;
            }
            set
            {
                this.pickItems = value;
                RaisePropertyChanged();
            }
        }

    }

    public enum PickFulfillStatus
    {
        Fulfilled,
        Fulfillable,
        Partial,
        None
    }

    public enum PickStatus
    {
        Entered = 10,
        Started = 20,
        Committed = 30,
        Finished = 40,
        AllOpen = 100,
        All = 120
    }

    /// <summary>
    /// Filters to be used to filter the pick list request
    /// </summary>
    public class PickListFilters
    {
        public string LocationGroupName { get; set; }
        public string Username { get; set; }
        public PickStatus Status { get; set; }
        public bool CompletelyFulfillable { get; set; }
        public string Carrier { get; set; }
    }

    

    public class PickItemLocations
    {
        public string Location { get; set; }
        public string TagNumber { get; set; }

    }

    public class PickItemInfo
    {
        public string PartNum { get; set; }
        public int Qty { get; set; }
        public string Location { get; set; }
        public string PickItemID { get; set; }
        public string LocationTag { get; set; }
        public int status { get; set; }

    }
}
