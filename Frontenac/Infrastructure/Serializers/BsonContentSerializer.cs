using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Frontenac.Infrastructure.Serializers
{
    public class BsonContentSerializer : IContentSerializer
    {
        private readonly JsonSerializer _serializer;

        public BsonContentSerializer()
        {
            _serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.All};
        }

        public bool IsBinary => true;

        public byte[] Serialize(object value)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonDataWriter(ms))
            {
                _serializer.Serialize(writer, new BsonWrapper {Data = value});
                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] raw)
        {
            using (var reader = new BsonDataReader(new MemoryStream(raw)))
            {
                return ((BsonWrapper) _serializer.Deserialize(reader)).Data;
            }
        }

        public class BsonWrapper
        {
            public object Data { get; set; }
        }
    }
}