using FishbowlConnect.Helpers;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class Shipping : NotifyOnChange
    {
        public string ID { get; set; }
        public string OrderNumber { get; set; }
        public string OrderType { get; set; }
        public string CreatedDate { get; set; }
        public string DateLastModified { get; set; }
        public string Carrier { get; set; }
        public string Status { get; set; }
        public string FOB { get; set; }
        public string Note { get; set; }
        public string CartonCount { get; set; }
        public string Contact { get; set; }

        public Address Address { get; set; }

        private Cartons cartons;

        public Cartons Cartons
        {
            get { return cartons; }
            set
            {
                cartons = value;
                RaisePropertyChanged();
            }
        }

        public FullyObservableCollection<ShippingItem> ItemsFlattened => 
            new FullyObservableCollection<ShippingItem>( Cartons?.Carton?.SelectMany(x => x.ShippingItems?.ShippingItem) );
    }

    public partial class Cartons
    {
        [JsonConverter(typeof(FullyObservableCollectionOrSingleValueConverter<Carton>))]
        public FullyObservableCollection<Carton> Carton { get; set; }
    }

    public partial class Carton : NotifyOnChange
    {
        private ShippingItems shippingItemsField;

        public string ID { get; set; }
        public string ShipID { get; set; }
        public string CartonNum { get; set; }
        public string TrackingNum { get; set; }
        public string FreightWeight { get; set; }
        public string FreightAmount { get; set; }

        public ShippingItems ShippingItems
        {
            get
            {
                return this.shippingItemsField;
            }
            set
            {
                this.shippingItemsField = value;
                RaisePropertyChanged();
            }
        }
    }

    public partial class ShippingItems
    {
        [JsonConverter(typeof(FullyObservableCollectionOrSingleValueConverter<ShippingItem>))]
        public FullyObservableCollection<ShippingItem> ShippingItem { get; set; }
    }

    public partial class ShippingItem : NotifyOnChange
    {

        public string ShipItemID { get; set; }
        public string ProductNumber { get; set; }
        public string ProductDescription { get; set; }
        public string QtyShipped { get; set; }
        public UOM UOM { get; set; }
        public string Cost { get; set; }
        public string SKU { get; set; }
        public string UPC { get; set; }
        public string OrderItemID { get; set; }
        public string OrderLineItem { get; set; }
        public string CartonName { get; set; }
        public string CartonID { get; set; }
        public string TagNum { get; set; }
        public string Weight { get; set; }
        public WeightUOM WeightUOM { get; set; }
        public string DisplayWeight { get; set; }
        public DisplayWeightUOM DisplayWeightUOM { get; set; }

        [JsonConverter(typeof(ListOrSingleValueConverter<TrackingItem>))]
        public List<TrackingItem> Tracking { get; set; }
    }

    public partial class ShipmentImage : NotifyOnChange
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int RecordId { get; set; }
        public int FileNumber { get; set; }

    }


    public class ShipListFilters
    {
        public ShipStatus Status { get; set; }
    }

    public enum ShipStatus
    {
        Entered = 10,
        Packed = 20,
        Shipped = 30,
        Cancelled = 40,
        AllOpen = 100,
        All = 120
    }
}
