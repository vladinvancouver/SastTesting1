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
    public class FilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly StorageService _storageService;
        private readonly EventService _eventService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly CallerService _callerService;

        public FilesController(ILogger<AccountsController> logger, StorageService storageService, EventService eventService, JsonSerializerOptions jsonSerializerOptions, CallerService callerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _callerService = callerService ?? throw new ArgumentNullException(nameof(callerService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        [Route("GetAll")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(string relativePath)
        {
            try
            {
                IEnumerable<DataTransfer.Objects.File> files = await FileRules.GetAllAsync(relativePath);
                return Ok(files);
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
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(string relativePath, string fileName)
        {
            try
            {
                await FileRules.DeleteAsync(relativePath, fileName);
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


        [Route("Download")]
        [HttpGet]
        public async Task<IActionResult> DownloadAsync(string relativePath, string fileName)
        {
            try
            {
                //Do not dispose stream.
                Stream stream = await FileRules.DownloadAsync(relativePath, fileName);
                stream.Position = 0;
                FileStreamResult fileStreamResult = File(stream, "application/octet-stream", fileName);
                return fileStreamResult;
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

    }
}
