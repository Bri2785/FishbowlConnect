using FishbowlConnect.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.MySQL
{
    public class InvQtyWithTracking : NotifyOnChange
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
        private string primaryTracking;

        public string PrimaryTracking
        {
            get { return primaryTracking; }
            set
            {
                primaryTracking = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TrackingValueAndName));
            }
        }
        private string primaryTrackingLabel;

        public string PrimaryTrackingLabel
        {
            get { return primaryTrackingLabel; }
            set
            {
                primaryTrackingLabel = value;
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

        public string TrackingValueAndName
        {
            get
            {
                return PrimaryTracking != null ? PrimaryTracking + "-" + TrackingAbbr : "";
            }
        }

        public string LocationFullName { get { return LocationGroupName + "-" + LocationName; } }
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
