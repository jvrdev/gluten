
namespace Gluten.Messages
{
    /// <summary>
    /// Interface to be implemented by request messages
    /// used in RPC-like interactions
    /// </summary>
    public interface IRequest : IMessage
    {
        string ResponseAddress { get; set; }
    }
}
