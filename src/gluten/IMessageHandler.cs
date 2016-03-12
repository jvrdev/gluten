using Gluten.Messages;

namespace Gluten
{
    /// <summary>
    ///     Handlers are invoked by the framework automatically when
    ///     a message of the proper type reaches the endpoint.
    ///     Handlers are instantiated by the framework for processing
    ///     just one message, they are destroyed immediately after that.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<in TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// Method invoked by framework when message of type TMessage is received.
        /// </summary>
        /// <param name="message"></param>
        void Handle(TMessage message);
    }
}
