using System;

namespace Gluten.Configuration
{
    public interface IResourceNameFactory
    {
        string CreateInputQueueName(string queueKey);
        string CreateTopicName(Type type);
        string CreateResponseQueueName();
        string CreateEventTopicMask();
    }
}
