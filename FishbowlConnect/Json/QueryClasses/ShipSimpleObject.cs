using FishbowlConnect.Json.APIObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    
    public class ShipSimpleObject : NotifyOnChange
    {
        string _dateShipped;

        public int ShipId { get; set; }
        public string ShipNum { get; set; }
        private string orderInfo;

        public string OrderInfo
        {
            get { return orderInfo; }
            set
            {
                orderInfo = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameAndPO));
            }
        }

        private string poNumber;

        public string PONumber
        {
            get { return poNumber; }
            set
            {
                poNumber = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameAndPO));
            }
        }

        public string Carrier { get; set; }


        public ShipStatus ShipStatus { get; set; }
        public string DateShipped {
            get
            {
                return Convert.ToDateTime(_dateShipped).ToShortDateString();
            }
            set
            {
                _dateShipped = value;
            }
        }
        public int CartonCount { get; set; }
        public int CustomerId { get; set; }

        [JsonIgnore]
        public string NameAndPO { get { return OrderInfo + PONumber ?? " - " + PONumber; } }
        [JsonIgnore]
        public string NumberAndNameAndPO { get { return ShipNum + " - " + OrderInfo + PONumber ?? " - " + PONumber; } }

        [JsonIgnore]
        public string OrderInfoInitial
        {
            get
            {
                if (string.IsNullOrEmpty(OrderInfo) || OrderInfo.Length == 0)
                {
                    return "?";
                }
                else
                {
                    return OrderInfo[0].ToString().ToUpper();
                }
            }
        }

    }

    //public class ShipSimpleObjectGroupedList : ObservableCollection<ShipSimpleObject>
    //{
    //    public string Heading { get; set; }
    //    public ObservableCollection<ShipSimpleObject> ShipSimpleObjects => this;
    //}

}
