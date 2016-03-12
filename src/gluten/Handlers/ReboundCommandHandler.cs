using log4net;
using Gluten.Messages;
using Gluten.Topology;

namespace Gluten.Handlers
{
    internal class ReboundCommandHandler<TCommand> : IMessageHandler<DelayedCommand<TCommand>>
        where TCommand : ICommand
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (ReboundCommandHandler<TCommand>));
        private readonly IPusher _pusher;

        public ReboundCommandHandler(IPusher pusher)
        {
            _pusher = pusher;
        }

        public void Handle(DelayedCommand<TCommand> message)
        {
            _pusher.PushDelayed(message.Command, message.RemainingDelay);
            _log.DebugFormat("Command of type {0} rebounded.", typeof(TCommand));
        }
    }
}
