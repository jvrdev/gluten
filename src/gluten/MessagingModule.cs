using Autofac;
using Gluten.Configuration;
using Gluten.Dispatch;
using Gluten.Handlers;
using Gluten.Messages;
using Gluten.Serialization;
using Gluten.Topology;
using System;
using System.Collections.Generic;
using ProtoBuf.Meta;

namespace Gluten
{
    /// <summary>
    /// Scaffolding of messaging components so that
    /// the same components do not need to be set up the same
    /// way over and over
    /// 
    /// In the future it will allow to configure the message
    /// processing pipe to support multithreading, transaction scopes,
    /// autoamtic performance counters, etc.
    /// </summary>
    public class MessagingModule : Module
    {
        private readonly string _environment;
        private readonly IDictionary<string, string> _topicMap;
        private readonly IRouteMapper _commandRouteMapper;
        private readonly IRouteMapper _requestRouteMapper;
        private readonly IEnumerable<Type> _subscriptions;
        private readonly string _inputQueueKey;

        public MessagingModule(IRouteMapper commandRouteMapper, IRouteMapper requestRouteMapper,
            IDictionary<string, string> topicMap, IEnumerable<Type> subscriptions,
            string environment, string inputQueueKey)
        {
            _topicMap = topicMap;
            _commandRouteMapper = commandRouteMapper;
            _requestRouteMapper = requestRouteMapper;
            _environment = environment;
            _subscriptions = subscriptions;
            _inputQueueKey = inputQueueKey;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var environmentParameter = new NamedParameter("environment", _environment);
            var topicMapParameter = new NamedParameter("topicMap", _topicMap);
            builder.RegisterType<AutofacMessageHandlerResolver>()
                .As<IMessageHandlerResolver>()
                .SingleInstance();
            builder.RegisterType<SqsMessageProcessor>()
                .As<ISqsMessageProcessor>()
                .SingleInstance();
            builder.RegisterType<JsonSerializer>()
                .As<IMessageSerializer>()
                .SingleInstance();
            builder.RegisterType<ResourceNameFactory>()
                .WithParameter(environmentParameter)
                .As<IResourceNameFactory>()
                .SingleInstance();
            builder.RegisterType<SqsQueueManager>()
                .As<IQueueManager>();
            builder.RegisterType<SqsEndpoint>()
                .WithParameter(topicMapParameter)
                .WithParameter("subscriptions", _subscriptions)
                .WithParameter("inputQueueKey", _inputQueueKey)
                .WithParameter("pollingThreadCount", 5)
                .AsSelf()
                .SingleInstance();
            builder.RegisterGeneric(typeof(SqsClient<,>))
                .WithParameter("routeMap", _requestRouteMapper)
                .As(typeof(IClient<,>));
            builder.RegisterType<SnsPublisher>()
                .WithParameter(topicMapParameter)
                .As<IPublisher>()
                .SingleInstance();
            builder.RegisterType<SqsPusher>()
                .WithParameter("routeMap", _commandRouteMapper)
                .As<IPusher>()
                .SingleInstance();
            builder.RegisterGeneric(typeof (ReboundCommandHandler<>))
                .AsImplementedInterfaces();
        }
    }
}
