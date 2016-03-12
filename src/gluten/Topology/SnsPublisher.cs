using Amazon.SimpleNotificationService;
using log4net;
using Gluten.Aws;
using Gluten.Configuration;
using Gluten.Messages;
using Gluten.Serialization;
using System.Collections.Generic;

namespace Gluten.Topology
{
    public class SnsPublisher : IPublisher
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SnsPublisher));
        private readonly IMessageSerializer _serializer;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IResourceNameFactory _nameFactory;
        private readonly IDictionary<string, string> _topicMap;

        public SnsPublisher(IAmazonSimpleNotificationService snsClient,
            IMessageSerializer serializer, IResourceNameFactory nameFactory,
            IDictionary<string, string> topicMap)
        {
            _snsClient = snsClient;
            _serializer = serializer;
            _nameFactory = nameFactory;
            _topicMap = topicMap;
        }

        /// <summary>
        /// Publish the event asynchronously
        /// </summary>
        /// <param name="event"></param>
        public void Publish<T>(T @event)
            where T : IEvent
        {
            var topicName = _nameFactory.CreateTopicName(typeof (T));
            var topicArn = _topicMap[topicName];
            var messageString = _serializer.GetString(@event);
            _snsClient.Publish(messageString, topicArn);
            _log.DebugFormat("Event of type {0} published on topic {0}.",
                topicArn);
        }
    }
}
