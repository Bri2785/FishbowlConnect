using FishbowlConnect.Helpers;
using FishbowlConnect.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.MySQL
{
    public class InvQtyWithAllTracking : NotifyOnChange, IPartTrackingFields
    {
        private string partNumber;
        public string PartNumber
        {
            get { return partNumber; }
            set
            {
                partNumber = value;
                RaisePropertyChanged();
            }
        }

        private decimal qty;
        public decimal Qty
        {
            get { return qty; }
            set
            {
                qty = value;
                RaisePropertyChanged();
            }
        }

        private int tagID;
        public int TagID
        {
            get { return tagID; }
            set
            {
                tagID = value;
                RaisePropertyChanged();
            }
        }

        private int locationId;
        public int LocationId
        {
            get { return locationId; }
            set
            {
                locationId = value;
                RaisePropertyChanged();
            }
        }

        private string locationName;
        public string LocationName
        {
            get { return locationName; }
            set
            {
                locationName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DefaultLocationMatchStatus));

            }
        }

        private bool locationPickable;
        public bool LocationPickable
        {
            get { return locationPickable; }
            set
            {
                locationPickable = value;
                RaisePropertyChanged();
            }
        }

        private string trackingInfo;
        public string TrackingInfo
        {
            get { return trackingInfo; }
            set
            {
                trackingInfo = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TrackingValueAndName));
            }
        }

        private string trackingLabel;
        public string TrackingLabel
        {
            get { return trackingLabel; }
            set
            {
                trackingLabel = value;
                RaisePropertyChanged();
            }
        }

        private string trackingAbbr;
        public string TrackingAbbr
        {
            get { return trackingAbbr; }
            set
            {
                trackingAbbr = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TrackingValueAndName));
            }
        }

        private int trackingTypeID;
        public int TrackingTypeID
        {
            get { return trackingTypeID; }
            set
            {
                trackingTypeID = value;
                RaisePropertyChanged();
            }
        }

        private int trackingID;
        public int TrackingID
        {
            get { return trackingID; }
            set
            {
                trackingID = value;
                RaisePropertyChanged();
            }
        }

        private int trackingSortOrder;

        public int TrackingSortOrder
        {
            get { return trackingSortOrder; }
            set
            {
                trackingSortOrder = value;
                RaisePropertyChanged();
            }
        }


        private bool isPrimaryTracking;
        public bool IsPrimaryTracking
        {
            get { return isPrimaryTracking; }
            set
            {
                isPrimaryTracking = value;
                RaisePropertyChanged();
            }
        }

        private string locationGroupName;
        public string LocationGroupName
        {
            get { return locationGroupName; }
            set
            {
                locationGroupName = value;
                RaisePropertyChanged();
            }
        }

        private int upcCaseQty;
        public int UPCCaseQty
        {
            get { return upcCaseQty; }
            set
            {
                upcCaseQty = value;
                RaisePropertyChanged();
            }
        }

        private string defaultLocationName;
        public string DefaultLocationName
        {
            get { return defaultLocationName; }
            set
            {
                defaultLocationName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DefaultLocationMatchStatus));
            }
        }

        public string TrackingValueAndName
        {
            get
            {
                return TrackingInfo != null ? TrackingInfo + "-" + TrackingAbbr : "";
            }
        }

        public string LocationFullName { get { return LocationGroupName + "-" + LocationName; } }

        public DefaultLocationMatchStatus DefaultLocationMatchStatus
        {
            get
            {
                if (LocationName == DefaultLocationName)
                {
                    return DefaultLocationMatchStatus.Matches;
                }
                else if (DefaultLocationName == null)
                {
                    return DefaultLocationMatchStatus.NotSet;
                }
                else
                {
                    return DefaultLocationMatchStatus.NotMatches;
                }
            }
        }

    }
}
