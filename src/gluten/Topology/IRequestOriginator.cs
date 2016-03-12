using Gluten.Messages;

namespace Gluten.Topology
{
    public interface IRequestOriginator
    {
        void Reply(IResponse response);
    }
}
