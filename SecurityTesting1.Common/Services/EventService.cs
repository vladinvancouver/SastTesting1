using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecurityTesting1.Common.Objects;

namespace SecurityTesting1.Common.Services
{
    public class EventService
    {
        //Using static so that all instances use the same storage. This is needed because
        //different instances are created by the application and plugin DI contains.
        //Data in the storage is then processed by a single background service.
        private static readonly ConcurrentQueue<EventMessage> _eventMessages = new();
        private readonly ILogger _logger;

        public EventService(ILogger<EventService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int Count => _eventMessages.Count;

        public void Enqueue(EventMessage eventMessage)
        {
            _eventMessages.Enqueue(eventMessage);
        }

        public IEnumerable<EventMessage> Dequeue(int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                if (_eventMessages.TryDequeue(out EventMessage? eventMessage))
                {
                    yield return eventMessage;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
