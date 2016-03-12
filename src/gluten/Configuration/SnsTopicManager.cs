using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleNotificationService;
using log4net;
using Gluten.Common;

namespace Gluten.Configuration
{
    public class SnsTopicManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (SnsTopicManager));

        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IDictionary<string, string> _topicMap =
            new Dictionary<string, string>();
        private readonly IResourceNameFactory _nameFactory;

        public SnsTopicManager(IAmazonSimpleNotificationService snsClient,
            IResourceNameFactory nameFactory)
        {
            _snsClient = snsClient;
            _nameFactory = nameFactory;
            Update();
        }

        private void Update()
        {
            _topicMap.Clear();
            var topicArns = _snsClient.ListTopics();

            foreach (var topicArn in topicArns.Topics)
            {
                var arnFields = topicArn.TopicArn.Split(':');
                var topic = arnFields.Last();
                _topicMap[topic] = topicArn.TopicArn;
            }
        }

        public IDictionary<string, string> CreateTopicMap(IEnumerable<Type> eventTypes)
        {
            var topicNames = eventTypes
                .Select(t => _nameFactory.CreateTopicName(t))
                .ToArray();
            var newTopicNames = topicNames
                .Where(tn => !_topicMap.ContainsKey(tn))
                .ToArray();

            _log.InfoFormat("Creating new topics [{0}]...", newTopicNames.ToCommaSeparatedStrings());
            foreach (var topicName in newTopicNames)
            {
                var topicArn = _snsClient.CreateTopic(topicName);
                _topicMap[topicName] = topicArn.TopicArn;
            }

            return new Dictionary<string, string>(_topicMap);
        }
    }
}
