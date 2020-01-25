using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    [Serializable()]
    public class Tag
    {
        public string TagID { get; set; }

        public string Num { get; set; }

        public string PartNum { get; set; }

        public string PartID { get; set; }

        public Location1 Location { get; set; }

        public decimal Quantity { get; set; }

        public string QuantityCommitted { get; set; }

        public string WONum { get; set; }

        public string DateCreated { get; set; }

        public Tracking Tracking { get; set; }

        public string TypeID { get; set; }

        public string AccountID { get; set; }

        public string TrackingFlag { get; set; }

    }


    /// <summary>
    /// Hold qty and location info for a specific tagID
    /// </summary>
    public class SimpleTag
    {
        public SimpleTag()
        {

        }
        public SimpleTag(int TagId, decimal Qty, decimal QtyCommitted, DateTime dateCreated, int LocationId = 0)
        {
            this.TagId = TagId;
            this.Quantity = Qty;
            this.QuantityCommitted = QtyCommitted;
            this.DateCreated = dateCreated;
            this.LocationId = LocationId;
        }
        public int TagId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityCommitted { get; set; }
        public DateTime DateCreated { get; set; }
        public int LocationId { get; set; }


    }

    class SimpleTagComparer : IEqualityComparer<SimpleTag>
    {
        public bool Equals(SimpleTag x, SimpleTag y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return
                x.TagId == y.TagId &&
                x.Quantity == y.Quantity &&
                x.LocationId == y.LocationId;
        }

        public int GetHashCode(SimpleTag obj)
        {
            //null check then creates hash from partnumber, tracking encoding, tag id

            if (ReferenceEquals(obj, null)) return 0;
            int hashSimpleTagID = obj.TagId == 0 ? 0 : obj.TagId.GetHashCode();
            int hashSimpleTagQty = obj.Quantity == decimal.Zero ? 0 : obj.Quantity.GetHashCode();
            int hashSimpleTagLocation = obj.LocationId == 0 ? 0 : obj.LocationId.GetHashCode();
            return hashSimpleTagID ^ hashSimpleTagQty ^ hashSimpleTagLocation;
        }
    }
}
