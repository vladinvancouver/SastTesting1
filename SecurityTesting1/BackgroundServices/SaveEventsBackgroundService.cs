using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using SecurityTesting1.Common.Services;
using SecurityTesting1.Common.Objects;
using SecurityTesting1.Common.Rules;

namespace SecurityTesting1.BackgroundServices
{
    public class SaveEventsBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly StorageService _storageService;
        private readonly EventService _eventService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SaveEventsBackgroundService(ILogger<SaveEventsBackgroundService> logger, StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Give time for application to start up all services.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch { }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<EventMessage> eventMessages = _eventService.Dequeue(maxCount: 100_000).ToList();

                    if (eventMessages.Any())
                    {
                        await EventEntryRules.SaveEventEntriesAsync(_storageService, _jsonSerializerOptions, eventMessages);
                    }
                    else
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        }
                        catch { };
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    await Task.Delay(TimeSpan.FromSeconds(10));  //In a loop, log file can fill up quickly unless we slow it down after error.
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            // Run your graceful clean-up actions
            await Task.CompletedTask;
        }
    }
}
