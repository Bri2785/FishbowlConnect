using FishbowlConnect.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    /// <summary>
    /// Product Simple Object hold a few of the available fields for a product
    /// </summary>
    public class ProductSimpleObject : NotifyOnChange, IPPSimple
    {
        private string number;

        public string Number
        {
            get { return number; }
            set
            {
                number = value;
                RaisePropertyChanged();
            }
        }
        private string description;

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
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

        private int uomId;
        public int UomId
        {
            get
            {
                return uomId;
            }
            set
            {
                uomId = value;
                RaisePropertyChanged();
            }
        }

        private string partNum;
        public string PartNum
        {
            get
            {
                return partNum;
            }
            set
            {
                partNum = value;
                RaisePropertyChanged();
            }
        }

    }
}
