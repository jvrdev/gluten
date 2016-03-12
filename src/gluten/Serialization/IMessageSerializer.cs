using System;

namespace Gluten.Serialization
{
    public interface IMessageSerializer
    {
        Type GetMessageType(byte[] buffer, int byteCount);
        dynamic GetMessage(byte[] buffer, int byteCount, Type type);
        byte[] GetBytes(object message);
    }
}
