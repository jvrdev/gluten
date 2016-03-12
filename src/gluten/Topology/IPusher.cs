using System;
using Gluten.Messages;

namespace Gluten.Topology
{
    /// <summary>
    /// Pushes commands to services.
    /// </summary>
    public interface IPusher
    {
        /// <summary>
        /// Pushes a command to be routed to a queue.
        /// Routing is controlled at configuration time through .Routing
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        void Push<T>(T command)
            where T : ICommand;

        /// <summary>
        /// Pushes a command to be routed to a queue with a delay.
        /// It allows scheduled execution of jobs.
        /// Routing is controlled at configuration time through .Routing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        void PushDelayed<T>(T command, TimeSpan delay)
            where T : ICommand;
    }
}
