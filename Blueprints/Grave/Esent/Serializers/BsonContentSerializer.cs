using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Frontenac.Grave.Esent.Serializers
{
    public class BsonContentSerializer : IContentSerializer
    {
        private readonly JsonSerializer _serializer;

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
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                _serializer.Serialize(writer, new BsonWrapper {Data = value});
                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] raw)
        {
            using (var reader = new BsonReader(new MemoryStream(raw)))
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