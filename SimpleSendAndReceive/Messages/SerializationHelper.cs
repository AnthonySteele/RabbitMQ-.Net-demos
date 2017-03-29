using Newtonsoft.Json;
using System.Text;

namespace Messages
{
    public static class SerializationHelper
    {
        public static byte[] ToByteArray<T>(this T t)
        {
            var objectstr = JsonConvert.SerializeObject(t);
            return Encoding.UTF8.GetBytes(objectstr);
        }

        public static object FromByteArray<T>(byte[] data)
        {
            var bodystr = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(bodystr);
        }
    }
}
