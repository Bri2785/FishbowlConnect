using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    [Serializable()]
    public class Tracking :NotifyOnChange
    {

        [JsonConverter(typeof(ListOrSingleValueConverter<TrackingItem>))]
        public List<TrackingItem> TrackingItem { get; set; }

        [JsonIgnore]
        public string PrimaryTrackingSummary
        {
            get { return TrackingItem?[0].TrackingSummary; }
        }
    }


    public class TrackingItem : NotifyOnChange
    {
        private string trackingValueField;

        /// <remarks/>
        public PartTracking PartTracking { get; set; }

        [JsonConverter(typeof(MySQLCompatibleDateFormat))]
        public string TrackingValue
        {
            get
            {
                return this.trackingValueField;
            }
            set
            {
                this.trackingValueField = value;
                RaisePropertyChanged(nameof(TrackingSummary));
            }
        }

        [JsonIgnore]
            public string TrackingSummary
            {
                get
                {
                    if (PartTracking.TrackingTypeID == "20" || PartTracking.TrackingTypeID == "30")
                    {
                        if (!string.IsNullOrEmpty(TrackingValue))
                        {
                            return PartTracking.Abbr + " - " + Convert.ToDateTime(TrackingValue).ToString("d");
                        }

                    }

                    return PartTracking.Abbr + " - " + TrackingValue;


                }

            }
        

    }

    public class TrackingSimple : NotifyOnChange, IPartTrackingFields
    {
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

        public TrackingSimple(string trackingInfo, string trackingLabel, string trackingAbbr, int trackingTypeID, int trackingID, int trackingSortOrder, bool isPrimaryTracking)
        {
            TrackingInfo = trackingInfo;
            TrackingLabel = trackingLabel;
            TrackingAbbr = trackingAbbr;
            TrackingTypeID = trackingTypeID;
            TrackingID = trackingID;
            TrackingSortOrder = trackingSortOrder;
            IsPrimaryTracking = isPrimaryTracking;
        }

        public bool IsPrimaryTracking
        {
            get { return isPrimaryTracking; }
            set
            {
                isPrimaryTracking = value;
                RaisePropertyChanged();
            }
        }

        public string TrackingValueAndName
        {
            get
            {
                return TrackingInfo != null ? TrackingInfo + "-" + TrackingAbbr : "";
            }
        }

    }

    class SimpleTrackingComparer : IEqualityComparer<TrackingSimple>
    {
        public bool Equals(TrackingSimple x, TrackingSimple y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return
                x.TrackingID == y.TrackingID &&
                x.TrackingInfo == y.TrackingInfo;
        }

        public int GetHashCode(TrackingSimple obj)
        {
            //null check then creates hash from partnumber, tracking encoding, tag id

            if (ReferenceEquals(obj, null)) return 0;
            int hashSimpleTrackingID = obj.TrackingID == 0 ? 0 : obj.TrackingID.GetHashCode();
            int hashSimpleTrackingInfo = obj.TrackingInfo == null ? 0 : obj.TrackingInfo.GetHashCode();
            return hashSimpleTrackingID ^ hashSimpleTrackingInfo;
        }
    }


}
