using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using Autofac;
using log4net;
using Gluten.Messages;
using Gluten.Serialization;
using System.Reflection;
using Gluten.Topology;
using Amazon.SQS;

namespace Gluten.Dispatch
{
    public class SqsMessageProcessor : ISqsMessageProcessor
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SqsMessageProcessor));
        private readonly IMessageSerializer _serializer;
        private readonly IMessageHandlerResolver _messageHandlerResolver;
        private readonly ILifetimeScope _iocContainer;
        private readonly IAmazonSQS _sqsClient;

        public SqsMessageProcessor(IAmazonSQS sqsClient, IMessageSerializer serializer,
            IMessageHandlerResolver messageHandlerResolver,
            ILifetimeScope iocContainer)
        {
            _sqsClient = sqsClient;
            _serializer = serializer;
            _messageHandlerResolver = messageHandlerResolver;
            _iocContainer = iocContainer;
        }

        public bool ProcessMessage(Message sqsMessage)
        {
            dynamic message;
            if (TryGetMessageFromSqsMessage(sqsMessage, out message))
            {
                using (var lifetimeScope = _iocContainer.BeginLifetimeScope())
                {
                    var handlers = TryGetMessageHandlers(message, lifetimeScope);
                    ConfigureHandlers(handlers, message);
                    foreach (var handler in handlers)
                    {
                        var success = TryHandlingMessage(handler, message);
                        if (!success) return false;
                    }
                }
            }

            return true;
        }

        private bool TryGetMessageFromSqsMessage(Message sqsMessage, out dynamic message)
        {
            try
            {
                var messageBody = sqsMessage.Body;
                message = _serializer.GetMessage(messageBody);
            }
            catch (Exception exception)
            {
                HandleMessageParsingException(exception);
                message = null;

                return false;
            }

            return true;
        }

        private IEnumerable<IMessageHandler<T>> TryGetMessageHandlers<T>(T message, ILifetimeScope lifetimeScope)
            where T : IMessage
        {
            IEnumerable<IMessageHandler<T>> handlers;
            var type = message.GetType();
            try
            {
                handlers = (IEnumerable<IMessageHandler<T>>)_messageHandlerResolver.GetHandlersFor(type, lifetimeScope);
                LogLoadedHandlers(handlers, type);
            }
            catch (Exception e)
            {
                HandleHandlerCreationException(e);
                handlers = new IMessageHandler<T>[0];
            }

            return handlers;
        }

        private void ConfigureHandlers<T>(IEnumerable<IMessageHandler<T>> handlers, T message)
            where T : IMessage
        {
            if (IsRequest<T>())
            {
                var request = message as IRequest;
                if (request == null) throw new ApplicationException(
                    string.Format("IsRequest failed for type {0}.", typeof(T)));
                var responseQueueUrl = request.ResponseAddress;
                foreach (var handler in handlers.Where(h => HasOriginator(h.GetType())))
                {
                    InjectOriginator(handler, responseQueueUrl);
                }
            }
        }

        private bool TryHandlingMessage<T>(IMessageHandler<T> handler, T message)
            where T : IMessage
        {
            try
            {
                handler.Handle(message);
                return true;
            }
            catch (Exception exception)
            {
                HandleMessageHandlingException(exception, message, handler);
            }

            return false;
        }

        private void HandleMessageParsingException(Exception exception)
        {
            _log.Error("Message could not be deserialized.", exception);
        }

        private void HandleHandlerCreationException(Exception exception)
        {
            _log.Fatal("Message handler could not be created, please check your IoC component registration.",
                exception);
        }

        private void LogLoadedHandlers<T>(IEnumerable<IMessageHandler<T>> handlers, Type type)
            where T : IMessage
        {
            if (!handlers.Any())
            {
                _log.WarnFormat("No handlers found for message of type {0}.", type);
            }
            else
            {
                var formattedHandlers = handlers.Aggregate(string.Empty, (s, h) => s + h.GetType().FullName + ", ");
                formattedHandlers = formattedHandlers.Substring(0, formattedHandlers.Length - 2);
                _log.DebugFormat("The following handlers were found for message of type {0}: {1}.", type, formattedHandlers);
            }
        }

        private void HandleMessageHandlingException(Exception exception, IMessage message, object handler)
        {
            _log.ErrorFormat("Message {0} could not be processed at handler {1}. Exception details: {2}.",
                message.ToPretty(), handler, exception);
        }

        #region RPC utils
        private bool IsRequest<T>()
        {
            var isRequest = typeof(T).GetInterfaces()
                .Any(t => t == typeof(IRequest));

            return isRequest;
        }

        private bool HasOriginator(Type type)
        {
            var hasOriginator = type.GetProperties()
                .Any(IsOriginator);

            return hasOriginator;
        }

        private void InjectOriginator(object handler, string responseQueueUrl)
        {
            var originatorProperty = handler.GetType()
                .GetProperties()
                .First(IsOriginator);
            // TODO: inject response queue URL
            var originator = new SqsRequestOriginator(_sqsClient, responseQueueUrl, _serializer);
            originatorProperty.SetValue(handler, originator, null);
        }

        private bool IsOriginator(PropertyInfo property)
        {
            var isOriginator = property.PropertyType == typeof(IRequestOriginator) &&
                    property.Name == "Originator";

            return isOriginator;
        }
        #endregion
    }
}
