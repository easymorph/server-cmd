using MorphSDK.Dto.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{
    internal static class JsonSerializationHelper
    {
        public static T Deserialize<T>(string input)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                var d = Encoding.UTF8.GetBytes(input);
                using (var ms = new MemoryStream(d))
                {
                    var data = (T)serializer.ReadObject(ms);
                    return data;
                }
            }
            catch (Exception ex)
            {
               
                return default(T);
            }
        }
        public static string Serialize<T>(T obj)
        {

            var serializer = new DataContractJsonSerializer(typeof(T));            
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                byte[] json = ms.ToArray();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }            

        }
    }
}
