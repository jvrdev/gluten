using Gluten.Messages;

namespace Gluten.Topology
{
    public interface IPublisher
    {
        void Publish<T>(T @event)
            where T : IEvent;
    }
}
