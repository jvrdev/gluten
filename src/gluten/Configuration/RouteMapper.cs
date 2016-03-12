using System;
using System.Collections.Generic;
using Gluten.Messages;

namespace Gluten.Configuration
{
    public class RouteMapper : IRouteMapper
    {
        private readonly IDictionary<Type, string> _map;

        public RouteMapper()
        {
            _map = new Dictionary<Type, string>();
        }

        public void AddRoute(Type type, string queueUrl)
        {
            _map[type] = queueUrl;
        }

        public string GetQueueUrl<TMessage>()
            where TMessage : IMessage
        {
            return GetQueueUrl(typeof(TMessage));
        }

        private string GetQueueUrl(Type type)
        {
            if (!_map.ContainsKey(type))
            {
                var message = string.Format("Route for message of type {0} was not found.", type);
                throw new ArgumentException(message);
            }

            return _map[type];
        }
    }
}
