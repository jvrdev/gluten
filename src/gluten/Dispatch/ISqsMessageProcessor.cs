using Amazon.SQS.Model;

namespace Gluten.Dispatch
{
    public interface ISqsMessageProcessor
    {
        bool ProcessMessage(Message message);
    }
}
