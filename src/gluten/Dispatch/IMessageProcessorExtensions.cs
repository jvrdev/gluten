using System;
using Gluten.Messages;

namespace Gluten.Dispatch
{
    public static class IMessageProcessorExtensions
    {
        /// <summary>
        /// Sugar around byte oriented method
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="this"></param>
        /// <param name="messageString"></param>
        public static void ProcessMessage<TMessage>(this IMessageProcessor<TMessage> @this, string messageString)
            where TMessage : IMessage
        {
            var messageBytes = Convert.FromBase64String(messageString);

            @this.ProcessMessage(messageBytes, messageBytes.Length);
        }
    }
}
