using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecurityTesting1.Common.Services;
using SecurityTesting1.Common.Rules;
using SecurityTesting1.DataTransfer.Objects;

namespace SecurityTesting1.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly StorageService _storageService;
        private readonly EventService _eventService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly CallerService _callerService;

        private User _user = null!;

        public UsersController(ILogger<UsersController> logger, StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, CallerService callerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _callerService = callerService ?? throw new ArgumentNullException(nameof(callerService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        [Route("Add")]
        [HttpPost]
        public IActionResult AddA([FromBody] User user)
        {
            try
            {
                _user = user;

                Validate();

                return Ok();
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
        public async Task Validate()
        {
            await Task.CompletedTask;

            if (_user is null)
                throw new ArgumentException($"Cannot read account.");

            if (string.IsNullOrWhiteSpace(_user.UserName))
                throw new ArgumentException($"'User name' is blank.");

            if (_user.Age < 18 && _user.Age > 100)
                throw new ArgumentException($"Your age does not fall within accepted range to use this service.");
        }
    }
}
