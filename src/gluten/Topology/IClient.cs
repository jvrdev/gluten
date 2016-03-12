using Gluten.Messages;

namespace Gluten.Topology
{
    public interface IClient<in TRequest, out TResponse>
        where TRequest : IRequest
        where TResponse : IResponse
    {
        TResponse Invoke(TRequest request);
    }
}
