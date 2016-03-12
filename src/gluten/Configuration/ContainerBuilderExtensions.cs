using Autofac;
using Autofac.Core;
using Gluten.Messages;

namespace Gluten.Configuration
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterMessageHandler<THandler, TMessage>(this ContainerBuilder builder)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            builder.RegisterType<THandler>()
                .As<IMessageHandler<TMessage>>();
        }

        public static void RegisterMessageHandler<THandler, TMessage>(this ContainerBuilder builder, params Parameter[] parameters)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            builder.RegisterType<THandler>()
                .WithParameters(parameters)
                .As<IMessageHandler<TMessage>>();
        }
    }
}
