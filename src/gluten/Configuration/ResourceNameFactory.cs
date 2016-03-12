using Gluten.Configuration;
using System;

namespace Gluten.Configuration
{
    public class ResourceNameFactory : IResourceNameFactory
    {
        private readonly string _environment;

        public ResourceNameFactory(string environment)
        {
            _environment = environment;
        }

        public string CreateInputQueueName(string queueKey)
        {
            var prefix = PrefixIndex.GetResourcePrefix(_environment);
            var fullQueueName = prefix + queueKey;

            return fullQueueName;
        }

        public string CreateTopicName(Type type)
        {
            var prefix = PrefixIndex.GetResourcePrefix(_environment);
            var topicName = prefix + type.FullName.Replace('.', '-');

            return topicName;
        }

        public string CreateTopicName<T>()
        {
            return CreateTopicName(typeof(T));
        }

        public string CreateResponseQueueName()
        {
            var key = "response-" + Guid.NewGuid();
            var name = CreateInputQueueName(key);

            return name;
        }

        public string CreateEventTopicMask()
        {
            var prefix = PrefixIndex.GetResourcePrefix(_environment);
            var mask = string.Format("arn:aws:sns:*:{0}:{1}Gluten-Messaging-Contracts-*Event",
                "719101807187",     // NOTE: all the topics are belong to us
                prefix);

            return mask;
        }
    }
}
