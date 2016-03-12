
namespace Gluten.Configuration
{
    public interface IQueueManager
    {
        string GetQueueArn(string queueName);
        string GetQueueUrl(string queueName);
    }
}
