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
    public class AccountsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly StorageService _storageService;
        private readonly EventService _eventService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly CallerService _callerService;

        public AccountsController(ILogger<AccountsController> logger, StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, CallerService callerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _callerService = callerService ?? throw new ArgumentNullException(nameof(callerService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        [Route("GetAll")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                IEnumerable<Account> accounts = await AccountRules.GetAllAsync(_storageService, _jsonSerializerOptions);
                return Ok(accounts);
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


        [Route("GetOrCreateAccount")]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> GetOrCreateAccountAsync([FromQuery] string accountName)
        {
            try
            {
                (Guid AccountId, bool IsCreated) = await AccountRules.GetOrCreateAccount(_storageService, _eventService, _jsonSerializerOptions, accountName, _callerService.PerformedBy, _callerService.FromRemoteIpAddress, _callerService.UserAgent);

                return Ok(new { AccountId, IsCreated });
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


        [Route("Add")]
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] Account account)
        {
            try
            {
                if (account == null)
                    return BadRequest($"Cannot read account.");

                if (string.IsNullOrWhiteSpace(account.AccountName))
                    return BadRequest($"'Account name' is blank.");

                if (account.AccountName.Length > 100)
                    return BadRequest($"'Account name' must be 100 characters or less.");

                if (string.IsNullOrWhiteSpace(account.Description))
                    return BadRequest($"'Description' is blank.");

                if (account.Description.Length > 100)
                    return BadRequest($"'Description' must be 100 characters or less.");

                Account dtAccount = await AccountRules.AddAccountAsync(_storageService, _eventService, _jsonSerializerOptions, account, _callerService.PerformedBy, _callerService.FromRemoteIpAddress, _callerService.UserAgent);

                return Ok(dtAccount);
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

        [Route("Update")]
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] Account account)
        {
            try
            {
                await AccountRules.UpdateAccountAsync(_storageService, _eventService, _jsonSerializerOptions, account, _callerService.PerformedBy, _callerService.FromRemoteIpAddress, _callerService.UserAgent);

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

        [Route("Delete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] Guid accountId)
        {
            try
            {
                await AccountRules.MarkAccountForDeletionAsync(_storageService, _eventService, _jsonSerializerOptions, accountId, _callerService.PerformedBy, _callerService.FromRemoteIpAddress, _callerService.UserAgent);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}
