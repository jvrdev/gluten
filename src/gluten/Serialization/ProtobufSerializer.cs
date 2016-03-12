using Gluten.Messages;
using ProtoBuf;
using System;
using System.IO;
using System.Text;

namespace Gluten.Serialization
{
    /// <summary>
    /// Format is:
    /// * 2 bytes fullTypeName length
    /// * n bytes fullTypeName in ASCII
    /// * m bytes for protobuf serialized message
    /// </summary>
    public class ProtobufSerializer : IMessageSerializer
    {
        public Type GetMessageType(byte[] buffer, int byteCount)
        {
            int offset;
            string fullTypeName = ReadFullTypeName(buffer, out offset);

            var type = ReadType(fullTypeName);
            return type;
        }

        public dynamic GetMessage(byte[] buffer, int byteCount, Type type)
        {
            int offset;
            ReadFullTypeName(buffer, out offset);
            var instance = Serializer.NonGeneric.Deserialize(type, new MemoryStream(buffer, offset, byteCount - offset));
            if (!(instance is IMessage))
            {
                throw new ApplicationException("Serialized object is not a message.");
            }

            return instance;
        }

        private string ReadFullTypeName(byte[] buffer, out int protobufOffset)
        {
            if (buffer.Length < 2)
            {
                throw new ArgumentException("buffer does not contain enough bytes to contain a valid message", "buffer");
            }
            ushort length = BitConverter.ToUInt16(buffer, 0);

            if (buffer.Length < (2 + length))
            {
                throw new ArgumentException("buffer does not contain enough bytes to contain a valid message", "buffer");
            }
            string fullTypeName = Encoding.ASCII.GetString(buffer, 2, length);

            protobufOffset = length + 2;

            return fullTypeName;
        }

        private Type ReadType(string fullTypeName)
        {
            Type type = null;

            try
            {
                type = Type.GetType(fullTypeName);
                if (type == null)
                {
                    HandleTypeReadingError<Exception>(fullTypeName, null);
                }
            }
            catch (Exception e)
            {
                HandleTypeReadingError(fullTypeName, e);
            }

            return type;
        }

        public byte[] GetBytes(object message)
        {
            byte[] fullMessageBytes;
            using (var memoryStream = new MemoryStream())
            {
                var type = message.GetType();
                var fullTypeName = type.AssemblyQualifiedName;
                var fullTypeNameBytes = Encoding.ASCII.GetBytes(fullTypeName);
                var fullTypeNameLengthBytes = BitConverter.GetBytes((UInt16)fullTypeNameBytes.Length);
                memoryStream.Write(fullTypeNameLengthBytes, 0, 2);
                memoryStream.Write(fullTypeNameBytes, 0, fullTypeNameBytes.Length);
                Serializer.Serialize(memoryStream, message);

                fullMessageBytes = memoryStream.ToArray();
            }

            return fullMessageBytes;
        }

        private void HandleTypeReadingError<T>(string typeName, T error)
            where T : Exception
        {
            var message = string.Format("Type {0} was not found.", typeName);
            var exception = error == null ?
                new ApplicationException(message) :
                new ApplicationException(message, error);

            throw exception;
        }
    }
}
