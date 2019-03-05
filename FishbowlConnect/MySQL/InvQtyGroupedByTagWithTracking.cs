using FishbowlConnect.Helpers;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishbowlConnect.MySQL
{
    public class InvQtyGroupedByTagWithTracking : NotifyOnChange
    {
        public InvQtyGroupedByTagWithTracking(string partNumber, decimal qty, int tagID, int locationId, string locationName, bool locationPickable, string locationGroupName, int upcCaseQty, string defaultLocationName, List<TrackingSimple> simpleTracking)
        {
            PartNumber = partNumber;
            Qty = qty;
            TagID = tagID;
            LocationId = locationId;
            LocationName = locationName;
            LocationPickable = locationPickable;
            LocationGroupName = locationGroupName;
            UPCCaseQty = upcCaseQty;
            DefaultLocationName = defaultLocationName;
            SimpleTracking = simpleTracking;
        }


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

        private List<TrackingSimple> simpleTracking;

        public List<TrackingSimple> SimpleTracking
        {
            get { return simpleTracking; }
            set
            {
                simpleTracking = value;
                RaisePropertyChanged();
            }
        }

        public string PrimaryTrackingValueAndName
        {
            get
            {
                if (SimpleTracking != null && SimpleTracking.Count > 0) //not null or empty
                {
                    //there are 3 states, no tracking (excluded on first check), tracking with primary values, tracking with no primary
                    TrackingSimple primary =
                             SimpleTracking
                                .Where(i => (i.TrackingInfo != null && i.IsPrimaryTracking)) //assigned tracking and primary
                                    
                                .FirstOrDefault();

                    if (primary == null)
                    {
                        primary = SimpleTracking
                                .Where(i => i.TrackingInfo != null && i.TrackingSortOrder == 1) //grabs the first one as the primary
                                .FirstOrDefault();
                    }

                    PrimaryTrackingValue = primary?.TrackingInfo;
                    return primary?.TrackingValueAndName;

                }
                return null;
            }
        }

        private string primaryTrackingValue;

        public string PrimaryTrackingValue
        {
            get { return primaryTrackingValue; }
            set
            {
                primaryTrackingValue = value;
                RaisePropertyChanged();
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
