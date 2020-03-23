using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.RequestClasses
{
    /// <summary>
    /// Request to save any image to the fishbowl database
    /// </summary>
    public class SaveApiImageRq
    {
        /// <summary>
        /// Base64 image
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// Record id for the assigned record
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// Table type, append API to beginning to avoid conflict with Fishbowl built-in
        /// </summary>
        public string Type { get; set; }
    }
}
