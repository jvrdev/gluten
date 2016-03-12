using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon.SQS;
using log4net;
using Gluten.Aws;
using Gluten.Messages;
using Gluten.Serialization;
using Gluten.Configuration;

namespace Gluten.Topology
{
    /// <summary>
    /// Thread-safe implementation
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class SqsClient<TRequest, TResponse> : IClient<TRequest, TResponse>
        where TRequest : IRequest
        where TResponse : IResponse
    {
        private readonly ILog _log;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _serverQueueUrl;
        private readonly IMessageSerializer _serializer;
        private readonly IResourceNameFactory _nameFactory;
        private readonly IQueueManager _queueResolver;

        public SqsClient(IAmazonSQS sqsClient, IQueueManager queueResolver,
            IMessageSerializer serializer, IResourceNameFactory nameFactory,
            IRouteMapper routeMap)
        {
            _log = LogManager.GetLogger(GetType());
            _sqsClient = sqsClient;
            _queueResolver = queueResolver;
            _serializer = serializer;
            _nameFactory = nameFactory;
            _serverQueueUrl = routeMap.GetQueueUrl<TRequest>();
        }

        public TResponse Invoke(TRequest request)
        {
            var responseQueueUrl = CreateResponseQueueUrl();
            try
            {
                request.ResponseAddress = responseQueueUrl;
                _log.DebugFormat("Sending request of type {0} to {1}...",
                    typeof(TRequest), _serverQueueUrl);
                SendRequest(request);
                var watch = Stopwatch.StartNew();
                var response = ReceiveResponse(responseQueueUrl);
                watch.Stop();
                _log.DebugFormat("Received response of type {0} from {1} after {2} ms.",
                    typeof(TResponse), _serverQueueUrl, watch.ElapsedMilliseconds);

                return response;
            }
            finally
            {
                DeleteResponseQueue(responseQueueUrl);
            }
        }

        private void SendRequest(TRequest request)
        {
            var message = _serializer.GetString(request);
            _sqsClient.SendMessage(message, _serverQueueUrl);
        }

        private TResponse ReceiveResponse(string responseQueueUrl)
        {
            var serializedResponse = _sqsClient.ReceiveMessageUnsafe(responseQueueUrl);
            var response = _serializer.GetMessage(serializedResponse);

            return response;
        }

        private string CreateResponseQueueUrl()
        {
            var queueName = _nameFactory.CreateResponseQueueName();
            var queueUrl = _queueResolver.GetQueueUrl(queueName);
            _log.DebugFormat("Created temporal RPC queue {0}.",
                queueUrl);

            return queueUrl;
        }

        private void DeleteResponseQueue(string queueUrl)
        {
            _sqsClient.DeleteQueue(queueUrl);
            _log.DebugFormat("Deleted temporal RPC queue {0}.",
                queueUrl);
        }
    }
}
