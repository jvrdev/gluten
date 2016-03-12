using System;
using Amazon.SQS;
using log4net;
using Gluten.Aws;
using Gluten.Messages;
using Gluten.Serialization;

namespace Gluten.Topology
{
    public class SqsRequestOriginator : IRequestOriginator
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (SqsRequestOriginator));
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;
        private readonly IMessageSerializer _serializer;

        public SqsRequestOriginator(IAmazonSQS sqsClient, string queueUrl, IMessageSerializer serializer)
        {
            _sqsClient = sqsClient;
            _queueUrl = queueUrl;
            _serializer = serializer;
        }

        public void Reply(IResponse response)
        {
            try
            {
                var message = _serializer.GetString(response);
                _sqsClient.SendMessage(message, _queueUrl);
                _log.DebugFormat("Response delivered to response queue with URL {0}.", _queueUrl);
            }
            catch (Exception e)
            {
                _log.WarnFormat("Response could not be delivered to client.\n{0}", e);
            }
        }
    }
}
