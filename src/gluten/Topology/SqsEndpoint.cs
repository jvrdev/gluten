using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using log4net;
using Gluten.Aws;
using Gluten.Configuration;
using Gluten.Dispatch;
using Gluten.Common;

namespace Gluten.Topology
{
    public class SqsEndpoint
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SqsEndpoint));

        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IResourceNameFactory _nameFactory;
        private readonly ISqsMessageProcessor _messageProcessor;
        private readonly IDictionary<string, string> _topicMap;
        private readonly IQueueManager _queueManager;
        private readonly string[] _subscriptions;
        private readonly string _inputQueueKey;
        private readonly int _pollingThreadCount;

        public SqsEndpoint(IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient,
            IResourceNameFactory nameFactory, ISqsMessageProcessor messageProcessor,
            IQueueManager queueManager, IDictionary<string, string> topicMap,
            IEnumerable<Type> subscriptions, string inputQueueKey, int pollingThreadCount)
        {
            _sqsClient = sqsClient;
            _nameFactory = nameFactory;
            _messageProcessor = messageProcessor;
            _topicMap = topicMap;
            _queueManager = queueManager;
            _inputQueueKey = inputQueueKey;
            _pollingThreadCount = pollingThreadCount;
            _subscriptions = subscriptions
                .Select(nameFactory.CreateTopicName)
                .ToArray();
            _snsClient = snsClient;
        }

        public void ProcessMessages()
        {
            var queueName = _nameFactory.CreateInputQueueName(_inputQueueKey);
            var queueUrl = _queueManager.GetQueueUrl(queueName);
            var queueArn = _queueManager.GetQueueArn(queueName);
            SubscribeQueueToTopics(queueArn);
            
            _log.InfoFormat("Starting endpoint with {0} threads polling from queue with URL {1}...",
                _pollingThreadCount, queueUrl);
            Parallel.For(0, _pollingThreadCount, i =>
            {
                for (;;)
                {
                    try
                    {
                        ProcessMessageBatch(queueUrl);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Processing of message batch failed at polling thread with id " +
                            i + ".", e);
                    }
                }
            });
        }

        private void SubscribeQueueToTopics(string queueArn)
        {
            _log.DebugFormat("Subscribing to topics [{0}].", _subscriptions.ToCommaSeparatedStrings());
            foreach (var topicName in _subscriptions)
            {
                var topicArn = _topicMap[topicName];

                SubscribeQueueToTopic(queueArn, topicArn);
            }
        }

        private void ProcessMessageBatch(string queueUrl)
        {
            var messages = _sqsClient.ReceiveMessages(queueUrl);
            var messagesToDelete = messages
                .AsParallel()
                .WithDegreeOfParallelism(AmazonSqsExtensions.MAX_MESSAGE_COUNT)
                .Where(m => _messageProcessor.ProcessMessage(m))
                .ToArray();

            if (messagesToDelete.Length > 0)
            {
                _sqsClient.DeleteMessages(messagesToDelete, queueUrl);
            }
        }

        private void SubscribeQueueToTopic(string queueArn, string topicArn)
        {
            var subscriptionArn = _snsClient.SubscribeQueueToTopic(queueArn, topicArn);
            _snsClient.ActivateRawMessageDelivery(subscriptionArn);
        }
    }
}