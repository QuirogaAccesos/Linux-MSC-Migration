using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Helper class for Enumerations
    /// </summary>
    public static class EnumHelper
    {
        /// <summary> Given a string find its matching enumeration </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetEnumValue<T>(string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }
            T val;
            return Enum.TryParse(str, true, out val) ? val : default(T);
        }

        /// <summary> Given an int find its matching enumeration </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }

            return (T)Enum.ToObject(enumType, intValue);
        }

        /// <summary> Give a character find its matching enumeration value </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="charValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetEnumValue<T>(char charValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }

            return (T)Enum.ToObject(enumType, charValue);
        }

        /// <summary> Given a long find its matching enumeration value </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="longValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetEnumValue<T>(long longValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }

            return (T)Enum.ToObject(enumType, longValue);
        }
    }
}
