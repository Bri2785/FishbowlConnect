
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace FishbowlConnect
{
    public class FBHelperClasses
    {
        public class ProductListInfo
        {
            //product number, product desc, product retail price, product dist price, product weight and dimensions, 
            //dist case qty, product image?

            public string ProductNumber { get; set; }
            public string ProductDesc { get; set; }
            public double RetailPrice { get; set; }
            public double DistPrice { get; set; }
            public double SprinklerPrice { get; set; }
            public int AFS { get; set; }
            public int QtyOnOrder { get; set; }
            public string ProductSpecs { get; set; }
            public string Weight { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string CaseQty { get; set; }
            public string ProductImage { get; set; }
        }

        public enum SaveImageType
        {
            Part,
            Product
        }

    }


    public static class MyExtensions
    {
        public static bool IsNull(this string input)
        {
            if (input == null)
            {
                return true;
            }
            else return false;
        }

        public static T DeepCopy<T>(T obj)

        {

            if (!typeof(T).IsSerializable)

            {

                throw new Exception("The source object must be serializable");

            }

            if (Object.ReferenceEquals(obj, null))

            {

                throw new Exception("The source object must not be null");

            }

            T result = default(T);

            using (var memoryStream = new MemoryStream())

            {

                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, obj);

                memoryStream.Seek(0, SeekOrigin.Begin);

                result = (T)formatter.Deserialize(memoryStream);

                memoryStream.Close();

            }

            return result;

        }

        public static T DeepCopyXML<T>(T obj)
        {
            var result = default(T);

            return result = FishbowlSession.DeserializeFromXMLString<T>(FishbowlSession.SerializeToXMLString(obj));
        }
    }



    public static class MySQLExtensions
    {
        public static T Get<T>(this IDataReader r, string columnName, T defaultValue = default(T))
        {
            var obj = r[columnName];
            if (obj == DBNull.Value)
                return defaultValue;

            return (T)obj;
        }
        public static T Get<T>(this IDataReader r, int columnIndex, T defaultValue = default(T))
        {
            var obj = r[columnIndex];
            if (obj == DBNull.Value)
                return defaultValue;

            return (T)obj;
        }
    }
}
