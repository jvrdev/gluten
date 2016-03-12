using System;

namespace Gluten.Serialization
{
    public static class IMessageSerializerExtensions
    {
        public static string GetString(this IMessageSerializer @this, object message)
        {
            var bytes = @this.GetBytes(message);
            var @string = Convert.ToBase64String(bytes);

            return @string;
        }

        public static dynamic GetMessage(this IMessageSerializer @this, string serializedMessage)
        {
            var bytes = Convert.FromBase64String(serializedMessage);
            var type = @this.GetMessageType(bytes, bytes.Length);
            var message = @this.GetMessage(bytes, bytes.Length, type);

            return message;
        }
    }
}
