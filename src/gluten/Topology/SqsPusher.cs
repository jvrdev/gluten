using System;
using Amazon.SQS;
using log4net;
using Gluten.Aws;
using Gluten.Configuration;
using Gluten.Messages;
using Gluten.Serialization;

namespace Gluten.Topology
{
    /// <summary>
    /// IPusher implementation built on top of AWS SQS
    /// </summary>
    public class SqsPusher : IPusher
    {
        private readonly TimeSpan _maxMessageDelay = TimeSpan.FromSeconds(AwsConstants.MAX_MESSAGE_DELAY_SECONDS);
        private readonly ILog _log = LogManager.GetLogger(typeof(SqsPusher));
        private readonly IMessageSerializer _serializer;
        private readonly IAmazonSQS _sqsClient;
        private readonly IRouteMapper _routeMap;

        public SqsPusher(IAmazonSQS sqsClient,
            IMessageSerializer serializer,
            IRouteMapper routeMap)
        {
            _sqsClient = sqsClient;
            _serializer = serializer;
            _routeMap = routeMap;
        }

        /// <summary>
        /// Pushes a command to be routed to a queue.
        /// Routing is controlled at configuration time through .Routing
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        public void Push<T>(T command)
            where T : ICommand
        {
            var queueUrl = _routeMap.GetQueueUrl<T>();
            var messageString = _serializer.GetString(command);
            _sqsClient.SendMessage(messageString, queueUrl);
            _log.DebugFormat("Command of type {0} pushed to queue {1}.",
                typeof(T), queueUrl);
        }

        /// <summary>
        /// Pushes a command to be routed to a queue with a delay.
        /// It allows scheduled execution of jobs.
        /// Routing is controlled at configuration time through .Routing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        public void PushDelayed<T>(T command, TimeSpan delay)
            where T : ICommand
        {
            var queueUrl = _routeMap.GetQueueUrl<T>();
            if (delay <= _maxMessageDelay)
            {
                var messageString = _serializer.GetString(command);
                _sqsClient.SendDelayedMessage(messageString, queueUrl, (int)delay.TotalSeconds);
                _log.DebugFormat("Command of type {0} pushed to queue {1} with a delay of {2} seconds.",
                    typeof(T), queueUrl, delay);
            }
            else
            {
                var remainingDelay = delay - _maxMessageDelay;
                var delayedCommand = new DelayedCommand<T>
                {
                    Command = command,
                    RemainingDelay = remainingDelay
                };
                var messageString = _serializer.GetString(delayedCommand);
                _sqsClient.SendDelayedMessage(messageString, queueUrl, (int)_maxMessageDelay.TotalSeconds);
                _log.DebugFormat("Command of type {0} pushed to queue {1} with a delay of {2} seconds.",
                    typeof(T), queueUrl, delay);
            }
        }
    }
}
