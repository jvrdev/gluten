using Autofac;
using System;
using System.Collections.Generic;

namespace Gluten.Dispatch
{
    public class AutofacMessageHandlerResolver : IMessageHandlerResolver
    {
        public dynamic GetHandlersFor(Type type, ILifetimeScope lifetimeScope)
        {
            var genericEnumerableType = typeof(IEnumerable<>);
            var genericHandlerType = typeof(IMessageHandler<>);
            var handlerType = genericHandlerType.MakeGenericType(type);
            var listOfHandlersType = genericEnumerableType.MakeGenericType(handlerType);
            var handlers = lifetimeScope.Resolve(listOfHandlersType);

            return handlers;
        }
    }
}
