using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Networking
{
    public static class SerializationHandler
    {
        private static readonly JsonSerializer Serializer;

        static SerializationHandler()
        {
            Serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        public static byte[] Serialize<T>(T data, int bufferSize) where T : class
        {
            byte[] buffer = new byte[bufferSize];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    Serializer.Serialize(sw, data);
                }
            }

            return buffer;
        }

        public static T Deserialize<T>(byte[] data) where T : class
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return Serializer.Deserialize(reader, typeof(T)) as T;
                }
            }
        }
    }
}