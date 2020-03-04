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

        public int PickID { get; set; }
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
        public int PickItemID { get; set; }
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

        public int SoItemId { get; set; }

        public int PoItemId { get; set; }

        public int XoItemId { get; set; }

        public int WoItemId { get; set; }

        public string SlotNumber { get; set; }

        public string Note { get; set; }

        public string AltNumber { get; set; }

        public long SourceTagID { get; set; }

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

    public class PickItemComparerWithoutTrackingFactor : IEqualityComparer<PickItem>
    {
        public bool Equals(PickItem x, PickItem y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return
                x.Part?.PartID == y.Part?.PartID &&
                x.SoItemId == y.SoItemId &&
                x.PoItemId == y.PoItemId &&
                x.XoItemId == y.XoItemId &&
                x.WoItemId == y.WoItemId &&
                x.Status.Equals(y.Status);
        }

        public int GetHashCode(PickItem obj)
        {
            //null check then creates hash from partnumber, tracking encoding, tag id

            if (ReferenceEquals(obj, null)) return 0;
            int hashSimplePartID = obj.Part.PartID == 0 ? 0 : obj.Part.PartID.GetHashCode();
            int hashStatus = obj.Status == null ? 0 : obj.Status.GetHashCode();
            return hashSimplePartID ^ obj.SoItemId.GetHashCode() ^
                obj.PoItemId.GetHashCode() ^
                obj.XoItemId.GetHashCode() ^
                obj.WoItemId.GetHashCode() ^ 
                hashStatus;
        }
    }

    /// <summary>
    /// Used when Fishbwowl splits items but the user want to pick all of them together. 
    /// You can use this to recombine the items for distinct tracking and part combo. 
    /// Main example: Used for printing after pick
    /// </summary>
    public class PickItemComparerIncludingTracking : IEqualityComparer<PickItem>
    {
        public bool Equals(PickItem x, PickItem y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            

            return
                x.Part?.PartID == y.Part?.PartID &&
                x.SoItemId == y.SoItemId &&
                x.PoItemId == y.PoItemId &&
                x.XoItemId == y.XoItemId &&
                x.WoItemId == y.WoItemId &&
                x.Status.Equals(y.Status) &&
                x.Tracking?.getEncoding() == y.Tracking?.getEncoding();
        }

        public int GetHashCode(PickItem obj)
        {
            //only load the trackingEncoding when comparing
            int hashTracking = 0;
            if (obj.Tracking != null)
            {
                obj.Tracking.TrackingEncoding = obj.Tracking?.getEncoding();
                hashTracking = obj.Tracking.TrackingEncoding == null ? 0 : obj.Tracking.TrackingEncoding.GetHashCode();
            }

            //null check then creates hash from partnumber, tracking encoding, tag id

            if (ReferenceEquals(obj, null)) return 0;
            int hashSimplePartID = obj.Part.PartID == 0 ? 0 : obj.Part.PartID.GetHashCode();
            int hashStatus = obj.Status == null ? 0 : obj.Status.GetHashCode();
            
            return hashSimplePartID ^ obj.SoItemId.GetHashCode() ^
                obj.PoItemId.GetHashCode() ^
                obj.XoItemId.GetHashCode() ^
                obj.WoItemId.GetHashCode() ^
                hashStatus ^ 
                hashTracking;
        }
    }

    /// <summary>
    /// Used when Fishbwowl splits items but the user want to pick all of them together. 
    /// You can use this to recombine the items to prevent extra unnecessary pick items
    /// Main example: User manually splits a pick item and ends up picking from same location and tracking anyways.
    /// </summary>
    public class PickItemComparerIncludingTrackingAndLocation : IEqualityComparer<PickItem>
    {
        public bool Equals(PickItem x, PickItem y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;



            return
                x.Part?.PartID == y.Part?.PartID &&
                x.SoItemId == y.SoItemId &&
                x.PoItemId == y.PoItemId &&
                x.XoItemId == y.XoItemId &&
                x.WoItemId == y.WoItemId &&
                x.Status.Equals(y.Status) &&
                x.Tracking?.getEncoding() == y.Tracking?.getEncoding() &&
                x.ItemStartedLocation == y.ItemStartedLocation;

        }

        public int GetHashCode(PickItem obj)
        {
            //only load the trackingEncoding when comparing
            int hashTracking = 0;
            if (obj.Tracking != null)
            {
                obj.Tracking.TrackingEncoding = obj.Tracking?.getEncoding();
                hashTracking = obj.Tracking.TrackingEncoding == null ? 0 : obj.Tracking.TrackingEncoding.GetHashCode();
            }

            //null check then creates hash from partnumber, tracking encoding, tag id

            if (ReferenceEquals(obj, null)) return 0;
            int hashSimplePartID = obj.Part.PartID == 0 ? 0 : obj.Part.PartID.GetHashCode();
            int hashStatus = obj.Status == null ? 0 : obj.Status.GetHashCode();
            int hashLocation = obj.ItemStartedLocation == null ? 0 : obj.ItemStartedLocation.GetHashCode();

            return hashSimplePartID ^ obj.SoItemId.GetHashCode() ^
                obj.PoItemId.GetHashCode() ^
                obj.XoItemId.GetHashCode() ^
                obj.WoItemId.GetHashCode() ^
                hashStatus ^ hashLocation ^
                hashTracking;
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

    public class VoidPickResponse
    {
        public Pick VoidedPick { get; set; }
        public string UnVoidableItems { get; set; }
    }

    public class ItemList
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<PickItem>))]
        public List<PickItem> PickItem { get; set; }
    }
}
