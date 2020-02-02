using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class Location1 : NotifyOnChange
    {
        private string nameField;
        private string locationGroupNameField;


        public string LocationID { get; set; }


        public string TypeID { get; set; }

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
                RaisePropertyChanged();
            }
        }

        /// <remarks/>
        public string Description { get; set; }

        /// <remarks/>
        public string CountedAsAvailable { get; set; }

        /// <remarks/>
        public string Active { get; set; }

        /// <remarks/>
        public string Pickable { get; set; }

        /// <remarks/>
        public string Receivable { get; set; }

        /// <remarks/>
        public string LocationGroupID { get; set; }

        /// <remarks/>
        public string LocationGroupName
        {
            get
            {
                return this.locationGroupNameField;
            }
            set
            {
                this.locationGroupNameField = value;
                RaisePropertyChanged();
            }
        }

        public bool Default { get; set; }

        /// <remarks/>
        public string SortOrder { get; set; }

        /// <remarks/>
        public string TagID { get; set; }

        public string TagNumber { get; set; }


        public string FullLocation
        {
            get
            {
                return LocationGroupName + " - " + Name;
            }
        }

        private bool _isSelected;

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

}
