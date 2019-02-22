using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    public class PickSimpleObject
    {
        string _dateScheduled;

        public string PickID { get; set; }
        public string PickNumber { get; set; }
        public string PickStatusID { get; set; }
        public string DateScheduled
        {
            get
            {
                return Convert.ToDateTime(_dateScheduled).ToShortDateString();
            }
            set
            {
                _dateScheduled = value;
            }
        }
        public string LocationGroupID { get; set; }
        public string Priority { get; set; }
        public string Username { get; set; }
        public string OrderInfo { get; set; }
        public string LGName { get; set; }
        public int ItemsNotFulfillable { get; set; }
        public int NumberOfItems { get; set; }

        public PickFulfillStatus PickFulfillibility
        {
            get
            {
                if (PickStatusID == "40")
                {
                    return PickFulfillStatus.Fulfilled;
                }
                else
                {
                    if (this.ItemsNotFulfillable == this.NumberOfItems)
                    {
                        return PickFulfillStatus.None;
                    }
                    else if (this.ItemsNotFulfillable == 0)
                    {
                        return PickFulfillStatus.Fulfillable;
                    }
                    else
                    {
                        return PickFulfillStatus.Partial;
                    }
                }
            }
        }

    }
}
