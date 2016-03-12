using Amazon.SQS;
using log4net;
using Gluten.Aws;
using System.Collections.Generic;

namespace Gluten.Configuration
{
    public class SqsQueueManager : IQueueManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SqsQueueManager));
        private readonly IAmazonSQS _sqsClient;
        private readonly IResourceNameFactory _nameFactory;

        public SqsQueueManager(IAmazonSQS sqsClient, IResourceNameFactory nameFactory)
        {
            _sqsClient = sqsClient;
            _nameFactory = nameFactory;
        }

        public string GetQueueUrl(string queueName)
        {
            string queueUrl = null;

            try
            {
                queueUrl = _sqsClient.GetQueueUrl(queueName).QueueUrl;
            }
            catch (KeyNotFoundException)
            {
                _log.WarnFormat("Queue with name {0} not found, creating queue...", queueName);
                queueUrl = CreateQueue(queueName);
            }

            return queueUrl;
        }

        public string GetQueueArn(string queueName)
        {
            var queueUrl = GetQueueUrl(queueName);
            var queueArn = _sqsClient.GetQueueArn(queueUrl);

            return queueArn;
        }

        private string CreateQueue(string queueName)
        {
            var queueUrl = _sqsClient.CreateQueue(queueName);
            _log.InfoFormat("Queue with name {0} successfullly created with URL {1}.",
                    queueName,  queueUrl);
            AddSendMessagePermissionToTopics(queueUrl.QueueUrl);

            return queueUrl.QueueUrl;
        }

        public void AddSendMessagePermissionToTopics(string queueUrl)
        {
            var queueArn = _sqsClient.GetQueueArn(queueUrl);
            var policy = CreatePolicy(queueUrl, queueArn);
            _sqsClient.SetQueueAttributesRequest(queueUrl, policy);
        }

        private string CreatePolicy(string queueUrl, string queueArn)
        {
            var topicMask = _nameFactory.CreateEventTopicMask();
            var policy = string.Format(
                "{{" +
                "   \"Version\": \"2012-10-17\"," +
                "   \"Id\": \"{0}/SubscriberPolicy\"," +
                "   \"Statement\": [" +
                "       {{" +
                "           \"Sid\": \"{0}\"," +
                "           \"Effect\": \"Allow\"," +
                "           \"Principal\": {{" +
                "               \"AWS\": \"*\"" +
                "           }}," +
                "           \"Action\": \"SQS:SendMessage\"," +
                "           \"Resource\": \"{2}\"," +
                "           \"Condition\": {{" +
                "               \"ArnLike\": {{" +
                "                   \"aws:SourceArn\": \"{1}\"" +
                "               }}" +
                "           }}" +
                "       }}" +
                "  ]" +
                "}}",
                queueUrl,
                topicMask,
                queueArn);

            return policy;
        }
    }
}