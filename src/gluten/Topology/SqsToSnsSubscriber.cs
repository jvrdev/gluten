using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using log4net;
using Gluten.Aws;

namespace Gluten.Topology
{
    public class SqsToSnsSubscriber
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SqsToSnsSubscriber));
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly string _queueUrl;

        public SqsToSnsSubscriber(IAmazonSimpleNotificationService snsClient,
            string queueUrl)
        {
            _snsClient = snsClient;
            _queueUrl = queueUrl;
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
            {
                _snsClient.SubscribeQueueToTopic(_queueUrl, topic);
                _log.DebugFormat("Subscribed to topic {0} at address {1}.",
                    topic, _queueUrl);
            }
        }
    }
}
