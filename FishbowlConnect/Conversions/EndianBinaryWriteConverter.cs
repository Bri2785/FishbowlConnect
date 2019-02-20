using System;
using System.IO;
using System.Text;


namespace FishbowlConnect.Helpers.Conversions
{
    /// <summary>
    /// Equivalent of System.IO.BinaryWriter, but with either endianness, depending on
    /// the EndianBitConverter it is constructed with.
    /// </summary>
    public class EndianBinaryWriteConverter : IDisposable
    {
        #region Fields not directly related to properties
        /// <summary>
        /// Whether or not this writer has been disposed yet.
        /// </summary>
        bool disposed = false;
        /// <summary>
        /// Buffer used for temporary storage during conversion from primitives
        /// </summary>
        byte[] buffer = new byte[16];
        /// <summary>
        /// Buffer used for Write(char)
        /// </summary>
        char[] charBuffer = new char[1];
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new binary writer with the given bit converter, writing
        /// to the given stream, using UTF-8 encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when writing data</param>
        /// <param name="stream">Stream to write data to</param>
        public EndianBinaryWriteConverter(EndianBitConverter bitConverter)
            : this(bitConverter, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Constructs a new binary writer with the given bit converter, writing
        /// to the given stream, using the given encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when writing data</param>
        /// <param name="stream">Stream to write data to</param>
        /// <param name="encoding">Encoding to use when writing character data</param>
        public EndianBinaryWriteConverter(EndianBitConverter bitConverter, Encoding encoding)
        {
            if (bitConverter == null)
            {
                throw new ArgumentNullException("bitConverter");
            }
            //if (stream == null)
            //{
            //    throw new ArgumentNullException("stream");
            //}
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            //if (!stream.CanWrite)
            //{
            //    throw new ArgumentException("Stream isn't writable", "stream");
            //}
            //this.stream = stream;
            this.bitConverter = bitConverter;
            this.encoding = encoding;
        }
        #endregion

        #region Properties
        EndianBitConverter bitConverter;
        /// <summary>
        /// The bit converter used to write values to the stream
        /// </summary>
        public EndianBitConverter BitConverter
        {
            get { return bitConverter; }
        }

        Encoding encoding;
        /// <summary>
        /// The encoding used to write strings
        /// </summary>
        public Encoding Encoding
        {
            get { return encoding; }
        }

        //Stream stream;
        ///// <summary>
        ///// Gets the underlying stream of the EndianBinaryConverter.
        ///// </summary>
        //public Stream BaseStream
        //{
        //    get { return stream; }
        //}
        #endregion

        #region Public methods
        /// <summary>
        /// Closes the writer, including the underlying stream.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        ///// <summary>
        ///// Flushes the underlying stream.
        ///// </summary>
        //public void Flush()
        //{
        //    CheckDisposed();
        //    stream.Flush();
        //}

        ///// <summary>
        ///// Seeks within the stream.
        ///// </summary>
        ///// <param name="offset">Offset to seek to.</param>
        ///// <param name="origin">Origin of seek operation.</param>
        //public void Seek(int offset, SeekOrigin origin)
        //{
        //    CheckDisposed();
        //    stream.Seek(offset, origin);
        //}

        /// <summary>
        /// Writes a boolean value to the stream. 1 byte is written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(bool value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the stream, using the bit converter
        /// for this writer. 2 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(short value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 2);
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(int value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(long value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 2 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(ushort value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 2);
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(uint value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes a 64-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(ulong value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a single-precision floating-point value to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(float value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes a double-precision floating-point value to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(double value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a decimal value to the stream, using the bit converter for this writer.
        /// 16 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(decimal value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            return WriteInternal(buffer, 16);
        }

        /// <summary>
        /// Writes a signed byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(byte value)
        {
            buffer[0] = value;
            return WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes an unsigned byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public byte[] GetByteArray(sbyte value)
        {
            buffer[0] = unchecked((byte)value);
            return WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes an array of bytes to the stream.
        /// </summary>
        /// <param name="value">The values to write</param>
        public byte[] GetByteArray(byte[] value)
        {
            if (value == null)
            {
                throw (new System.ArgumentNullException("value"));
            }
            return WriteInternal(value, value.Length);
        }

        ///// <summary>
        ///// Writes a portion of an array of bytes to the stream.
        ///// </summary>
        ///// <param name="value">An array containing the bytes to write</param>
        ///// <param name="offset">The index of the first byte to write within the array</param>
        ///// <param name="count">The number of bytes to write</param>
        //public byte[] GetByteArray(byte[] value, int offset, int count)
        //{
        //    CheckDisposed();
        //    stream.Write(value, offset, count);
        //}

        ///// <summary>
        ///// Writes a single character to the stream, using the encoding for this writer.
        ///// </summary>
        ///// <param name="value">The value to write</param>
        //public byte[] GetByteArray(char value)
        //{
        //    charBuffer[0] = value;
        //    return WriteInternal(charBuffer, 1);
        //}

        /// <summary>
        /// Writes an array of characters to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">An array containing the characters to write</param>
        public byte[] GetByteArray(char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CheckDisposed();
            byte[] data = Encoding.GetBytes(value, 0, value.Length);
            return WriteInternal(data, data.Length);
        }

        /// <summary>
        /// Writes a string to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">The value to write. Must not be null.</param>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public byte[] GetByteArray(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CheckDisposed();
            byte[] data = Encoding.GetBytes(value);

            
            byte[] dataLength = GetByteArray7BitEncodedInt(data.Length);  //combine these two arrays to send as one package
            byte[] returnedData = WriteInternal(data, data.Length);

            byte[] joined = new byte[dataLength.Length + returnedData.Length];
            System.Buffer.BlockCopy(dataLength, 0, joined, 0, dataLength.Length);
            System.Buffer.BlockCopy(returnedData, 0, joined, dataLength.Length, returnedData.Length);

            return joined;
        }

        /// <summary>
        /// Writes a 7-bit encoded integer from the stream. This is stored with the least significant
        /// information first, with 7 bits of information per byte of value, and the top
        /// bit as a continuation flag.
        /// </summary>
        /// <param name="value">The 7-bit encoded integer to write to the stream</param>
        public byte[] GetByteArray7BitEncodedInt(int value)
        {
            CheckDisposed();
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Value must be greater than or equal to 0.");
            }
            int index = 0;
            while (value >= 128)
            {
                buffer[index++] = (byte)((value & 0x7f) | 0x80);
                value = value >> 7;
                index++;
            }
            buffer[index++] = (byte)value;
            return WriteInternal(buffer, index);
        }



        #endregion

        #region Private methods
        /// <summary>
        /// Checks whether or not the writer has been disposed, throwing an exception if so.
        /// </summary>
        void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("EndianBinaryConverter");
            }
        }

        /// <summary>
        /// Writes the specified number of bytes from the start of the given byte array,
        /// after checking whether or not the writer has been disposed.
        /// </summary>
        /// <param name="bytes">The array of bytes to write from</param>
        /// <param name="length">The number of bytes to write</param>
        byte[] WriteInternal(byte[] bytes, int length)
        {
            CheckDisposed();
            return bytes.SubArray(0, length);
            //stream.Write(bytes, 0, length);
        }


        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes of the underlying stream.
        /// </summary>
        protected virtual void Dispose(bool Disposing)
        {
            if (!disposed)
            {
                //Flush();
                disposed = true;
                //((IDisposable)stream).Dispose();
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class ExtensionMethods
    {
        public static byte[] SubArray(this byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
