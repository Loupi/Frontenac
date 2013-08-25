using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Grave.Esent.Serializers
{
    public class BsonContentSerializer : IContentSerializer
    {
        readonly JsonSerializer _serializer;

        public BsonContentSerializer()
        {
            _serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.All};
        }

        public bool IsBinary
        {
            get { return true; }
        }

        public byte[] Serialize(object value)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BsonWriter(ms))
                {
                    _serializer.Serialize(writer, new BsonWrapper {Data = value});
                }

                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] raw)
        {
            using (var ms = new MemoryStream(raw))
            {
                using (var reader = new BsonReader(ms))
                {
                    return ((BsonWrapper)_serializer.Deserialize(reader)).Data;
                }
            }
        }

        public class BsonWrapper
        {
            public object Data { get; set; }
        }
    }
}
