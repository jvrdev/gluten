using System;
using ProtoBuf;

namespace Gluten.Messages
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DelayedCommand<TCommand> : ICommand
        where TCommand : ICommand
    {
        public TCommand Command { set; get; }
        public TimeSpan RemainingDelay { set; get; }
    }
}
