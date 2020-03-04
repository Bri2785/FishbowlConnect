using FishbowlConnect.Helpers;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        /// <summary>
        /// This can be set using the getEncoding method if needed
        /// </summary>
        [JsonIgnore]
        public string TrackingEncoding { get; set; }
        public string getEncoding()
        {

            string trackingString = TrackingToStringForEncoding();
            if (string.IsNullOrEmpty(trackingString))
            {
                return string.Empty;
            }
            try
            {
                //base64 convert then md5 hash
                byte[] trackingBytes = Encoding.UTF8.GetBytes(trackingString);
                string base64 = Convert.ToBase64String(trackingBytes);
                return base64.CreateMD5();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private string TrackingToStringForEncoding()
        {
            if (TrackingItem != null && TrackingItem.Count > 0)
            {

                List<TrackingItem> filteredAndSortedTracking = TrackingItem.Where(t => t.PartTracking.PartTrackingID != "30").ToList();
                    
                filteredAndSortedTracking.Sort(new Comparison<TrackingItem>((x, y) => x.PartTracking.PartTrackingID.CompareTo(y.PartTracking.PartTrackingID)));

                if (filteredAndSortedTracking.Count == 0)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                foreach (var item in filteredAndSortedTracking)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(item.TrackingSummary);
                }

                return sb.ToString();

            }
            return string.Empty;
        }


    }








        [JsonConverter(typeof(TrackingItemSerializeMySqlDateConverter))]
    public class TrackingItem : NotifyOnChange
    {
       
        /// <remarks/>
        public PartTracking PartTracking { get; set; }


        private string trackingValueField;
        //this is serialized by the type of the partTracking field
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

        private SerialBoxList serialBoxList;

        public SerialBoxList SerialBoxList
        {
            get { return serialBoxList; }
            set
            {
                serialBoxList = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public string TrackingSummary
        {
            get
            {
                if (PartTracking != null)
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
                return null;

            }
        }
        

    }

    public partial class PartTracking
    {

        public string PartTrackingID { get; set; }

        public string Name { get; set; }

        public string Abbr { get; set; }

         public string Description { get; set; }

        public string SortOrder { get; set; }

        public string TrackingTypeID { get; set; }

        public string Active { get; set; }

         public string Primary { get; set; }
    }

    public class SerialBoxList
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<SerialBox>))]
        public List<SerialBox> SerialBox { get; set; }
    }

    public class SerialBox
    {
        
        public SerialNumList SerialNumList { get; set; }

    }

    public class SerialNumList
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<SerialNum>))]
        public List<SerialNum> SerialNum { get; set; }
    }

    public partial class SerialNum
    {

        public int SerialID { get; set; }
        public int SerialNumID { get; set; }

        public string Number { get; set; }

        public PartTracking PartTracking { get; set; }
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
