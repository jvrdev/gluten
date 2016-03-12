using System;
using Autofac;

namespace Gluten.Dispatch
{
    public interface IMessageHandlerResolver
    {
        dynamic GetHandlersFor(Type type, ILifetimeScope lifetimeScope);
    }
}
