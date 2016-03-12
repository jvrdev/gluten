using Gluten.Messages;

namespace Gluten.Dispatch
{
    public interface IMessageProcessor<T>
        where T : IMessage
    {
        void ProcessMessage(byte[] buffer, int byteCount);
    }
}
