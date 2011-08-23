namespace Messages
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class SerializationHelper
    {
        public static byte[] ToByteArray<T>(this T t)
        {
            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, t);
                return memoryStream.ToArray();
            }
        }

        public static object FromByteArray(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }    
        }
    }
}
