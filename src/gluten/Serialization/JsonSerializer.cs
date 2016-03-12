using System;
using System.Text;
using Newtonsoft.Json;

namespace Gluten.Serialization
{
    // Deserialization is inefficient as it happens twice per message
    // however given the size of our payloads simplicity is preferred
    // An alternative is to use smarter type hinting
    public class JsonSerializer : IMessageSerializer
    {
        private Encoding _encoding = Encoding.UTF8;
        private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public byte[] GetBytes(object message)
        {
            var text = JsonConvert.SerializeObject(message, _jsonSettings);
            var bytes = _encoding.GetBytes(text);

            return bytes;
        }

        public dynamic GetMessage(byte[] buffer, int byteCount, Type type)
        {
            var text = _encoding.GetString(buffer, 0, byteCount);
            var instance = JsonConvert.DeserializeObject(ReadText(buffer, byteCount), type);

            return instance;
        }

        public Type GetMessageType(byte[] buffer, int byteCount)
        {
            // this will attempt to use Json.Net type annotation
            var instance = JsonConvert.DeserializeObject(ReadText(buffer, byteCount), _jsonSettings);
            var type = instance.GetType();

            return type;
        }

        private string ReadText(byte[] buffer, int byteCount)
        {
            var text = _encoding.GetString(buffer, 0, byteCount);

            return text;
        }
    }
}
