using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleNotificationService;
using Autofac;
using Gluten.Messages;
using Gluten.Topology;
using Amazon;
using Amazon.SQS;
using Gluten.Common;

namespace Gluten.Configuration
{
    public class EndpointConfigurationExpression : IEndpointConfigurationExpression
    {
        private readonly ContainerBuilder _builder = new ContainerBuilder();
        private readonly IList<Type> _subscriptions = new List<Type>();
        private readonly IList<Type> _publications = new List<Type>();
        private readonly IList<Tuple<Type, string>> _commandRoutes = new List<Tuple<Type, string>>();
        private readonly IList<Tuple<Type, string>> _requestRoutes = new List<Tuple<Type, string>>();
        private string _environment;

        private string _inputQueueKey = null;
        private RegionEndpoint _region = null;       // TODO: ugly hack
        private AmazonSQSClient _sqsClient;

        public IEndpointConfigurationExpression ForEnvironment(string environment)
        {
            _environment = environment;
            _sqsClient = new AmazonSQSClient(_region);

            return this;
        }
    
        public IEndpointConfigurationExpression InRegion(RegionEndpoint region)
        {
            _region = region;

            return this;
        }

        public IEndpointConfigurationExpression WithInputQueue(string inputQueueKey)
        {
            _inputQueueKey = inputQueueKey;

            return this;
        }

        public IEndpointConfigurationExpression WithHandlers<TModule>(TModule module)
            where TModule : Module
        {
            _builder.RegisterModule(module);

            return this;
        }

        public IEndpointConfigurationExpression SubscribingTo<TEvent>()
            where TEvent : IEvent
        {
            _subscriptions.Add(typeof(TEvent));

            return this;
        }

        public IEndpointConfigurationExpression Publishing<TEvent>()
            where TEvent : IEvent
        {
            _publications.Add(typeof(TEvent));

            return this;
        }

        public IEndpointConfigurationExpression Routing<TMessage>(string queueKey)
            where TMessage : IMessage
        {
            var messageType = typeof (TMessage);
            if (messageType.Implements<ICommand>())
            {
                _commandRoutes.Add(new Tuple<Type, string>(messageType, queueKey));
            }
            else if (messageType.Implements<IRequest>())
            {
                _requestRoutes.Add(new Tuple<Type, string>(messageType, queueKey));
            }
            else
            {
                var message = string.Format(
                        "The only message types that can be routed are commands and requests, {0} is neither.",
                        messageType);
                throw new ArgumentException(message, "TMessage");
            }

            return this;
        }

        public void Start()
        {
            _builder.RegisterModule(CreateModule());

            CheckStartableState();
            using (var container = _builder.Build())
            {
                CheckContainer(container);

                var endpoint = container.Resolve<SqsEndpoint>();
                endpoint.ProcessMessages();
            }
        }

        private void CheckContainer(IComponentContext container)
        {
            if (!container.IsRegistered<IAmazonSQS>())
            {
                throw new InvalidOperationException("AWS components are not registered in container...");
            }
        }

        public Module CreateModule()
        {
            CheckSourcesState();
            var nameFactory = new ResourceNameFactory(_environment);
            var topicMap = CreateTopics(nameFactory);
            var commandRouteMap = CreateRouteMap(nameFactory, _commandRoutes);
            var requestRouteMap = CreateRouteMap(nameFactory, _requestRoutes);

            return new MessagingModule(commandRouteMap, requestRouteMap, topicMap, _subscriptions,
                _environment, _inputQueueKey);
        }

        private void CheckStartableState()
        {
            if (string.IsNullOrWhiteSpace(_inputQueueKey))
            {
                throw new InvalidOperationException("Input queue not set.");
            }
        }

        private void CheckSourcesState()
        {
            if (string.IsNullOrWhiteSpace(_environment))
            {
                throw new InvalidOperationException("Environment not set.");
            }
            if (_region == null)
            {
                throw new InvalidOperationException("RegionEndpoint not set.");
            }
        }

        private IDictionary<string, string> CreateTopics(IResourceNameFactory nameFactory)
        {
            var snsClient = new AmazonSimpleNotificationServiceClient(_region);
            var topicManager = new SnsTopicManager(snsClient, nameFactory);
            var topicList = _publications.Union(_subscriptions);
            var topicMap = topicManager.CreateTopicMap(topicList);

            return topicMap;
        }

        private IRouteMapper CreateRouteMap(IResourceNameFactory nameFactory, IEnumerable<Tuple<Type, string>> routes)
        {
            var queueManager = new SqsQueueManager(_sqsClient, nameFactory);
            var map = new RouteMapper();
            foreach (var pair in routes)
            {
                var queueName = nameFactory.CreateInputQueueName(pair.Item2);
                var queueUrl = queueManager.GetQueueUrl(queueName);
                map.AddRoute(pair.Item1, queueUrl);
            }

            return map;
        }
    }
}
