
using System;

namespace Gluten.Messages
{
    /// <summary>
    /// Abstract DTO for Pub/Sub interactions
    /// </summary>
    public interface IEvent : IMessage
    {
        DateTimeOffset PublishedAt { get; set; }
    }
}
