using Gluten.Messages;
using System;

namespace Gluten.Configuration
{
    public interface IRouteMapper
    {
        void AddRoute(Type type, string queueUrl);
        string GetQueueUrl<TMessage>()
            where TMessage : IMessage;
    }
}
