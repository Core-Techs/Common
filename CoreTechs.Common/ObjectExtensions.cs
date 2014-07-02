using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CoreTechs.Common
{
    public static class ObjectExtensions
    {
        public static T Noop<T>(this T source)
        {
            return source;
        }

        /// <summary>
        /// Produces an encrypted byte array serialization of the value.
        /// Encryption achieved with System.Security.Cryptography.ProtectedData class.
        /// </summary>
        public static byte[] Encrypt<T>(this T value, DataProtectionScope scope = default(DataProtectionScope), byte[] entropy = null)
        {
            return ProtectedData.Protect(new Wrapper<T> { Value = value }.Serialize(), entropy, scope);
        }

        /// <summary>
        /// Deserializes and decrypts a previously encrypted, serialized value.
        /// Decryption achieved with System.Security.Cryptography.ProtectedData class.
        /// </summary>
        public static T Decrypt<T>(this IEnumerable<byte> bytes, DataProtectionScope scope = default(DataProtectionScope),
             byte[] entropy = null)
        {
            return ProtectedData.Unprotect(bytes.ToArray(), entropy, scope).Deserialize<Wrapper<T>>().Value;
        }

        /// <summary>
        /// Serializes an object using BinaryFormatter.
        /// </summary>
        public static byte[] Serialize<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes an object using BinaryFormatter.
        /// </summary>
        public static T Deserialize<T>(this byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
                return (T)new BinaryFormatter().Deserialize(ms);
        }

        /// <summary>
        /// Serializes an object using XmlSerializer.
        /// </summary>
        public static string SerializeToXML<T>(this T obj)
        {
            var xs = new XmlSerializer(typeof(T));
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
                xs.Serialize(tw, obj);

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes an object using XmlSerializer.
        /// </summary>
        public static T DeserializeFromXML<T>(this string xml)
        {
            var xs = new XmlSerializer(typeof(T));
            using (var tr = new StringReader(xml))
                return (T)xs.Deserialize(tr);
        }
    }

    [Serializable]
    public class Wrapper<T>
    {
        public T Value;
    }
}
