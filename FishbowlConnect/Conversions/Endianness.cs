
using System.ComponentModel;

namespace FishbowlConnect.Helpers.Conversions
{
    /// <summary>
    /// Endianness of a converter
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum Endianness
    {
        /// <summary>
        /// Little endian - least significant byte first
        /// </summary>
        LittleEndian,
        /// <summary>
        /// Big endian - most significant byte first
        /// </summary>
        BigEndian
    }
}
