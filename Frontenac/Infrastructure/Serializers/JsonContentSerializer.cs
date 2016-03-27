using System.Text;
using Newtonsoft.Json;

namespace Frontenac.Infrastructure.Serializers
{
    public class JsonContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        public bool IsBinary => true;

        public byte[] Serialize(object value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, _settings));
        }

        public object Deserialize(byte[] raw)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(raw), _settings);
        }
    }
}