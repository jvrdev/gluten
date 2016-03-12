using Autofac;
using Gluten.Messages;
using Amazon;

namespace Gluten.Configuration
{
    public interface IEndpointConfigurationExpression
    {
        IEndpointConfigurationExpression WithHandlers<TModule>(TModule module)
            where TModule : Module;
        IEndpointConfigurationExpression SubscribingTo<TEvent>()
            where TEvent : IEvent;
        IEndpointConfigurationExpression ForEnvironment(string environment);
        IEndpointConfigurationExpression WithInputQueue(string inputQueue);
        IEndpointConfigurationExpression InRegion(RegionEndpoint region);
        IEndpointConfigurationExpression Publishing<TEvent>()
            where TEvent : IEvent;
        IEndpointConfigurationExpression Routing<TMessage>(string queueKey)
            where TMessage : IMessage;
        
        void Start();
        Module CreateModule();
    }
}
