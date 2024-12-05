using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CatalogServiceAPI.Models;
using EnvironmentAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CatalogServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogInterface _catalogService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ICatalogInterface catalogService, ILogger<CatalogController> logger)
        {
            _catalogService = catalogService;
            _logger = logger;

            // Log service information
            LogServiceInformation();
        }

        private void LogServiceInformation()
        {
            // Get hostname and IP address
            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddresses(hostName);
            var ipAddress = ips.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

            // Log information about service
            if (!string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogInformation($"Catalog Service responding from {ipAddress}");
            }
            else
            {
                _logger.LogWarning("Unable to determine the IP address of the host.");
            }
        }

        [HttpGet("{_id}")]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<Catalog>> GetCatalog(Guid _id)
        {
            _logger.LogInformation($"Attempting to retrieve catalog with ID: {_id}");

            var catalog = await _catalogService.GetCatalog(_id);
            if (catalog == null)
            {
                _logger.LogWarning($"Catalog with ID {_id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Catalog with ID {_id} retrieved successfully");
            return catalog;
        }

        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<IEnumerable<Catalog>>> GetCatalogList()
        {
            _logger.LogInformation("Attempting to retrieve catalog list");

            var catalogList = await _catalogService.GetCatalogList();
            if (catalogList == null)
            {
                _logger.LogError("Catalog list is null");
                throw new ApplicationException("The list is null");
            }

            _logger.LogInformation("Catalog list retrieved successfully");
            return Ok(catalogList);
        }

        [HttpPost]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<Guid>> AddCatalog(Catalog catalog)
        {
            _logger.LogInformation("Attempting to add catalog");

            var _id = await _catalogService.AddCatalog(catalog);

            _logger.LogInformation($"Catalog added with ID: {_id}");

            return Ok($"Catalog added with ID: {_id}");
        }

        [HttpPut("{_id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> UpdateCatalog(Guid _id, Catalog catalog)
        {
            _logger.LogInformation($"Attempting to update catalog with ID: {_id}");

            if (_id != catalog._id)
            {
                _logger.LogWarning("ID in URL does not match ID in request body");
                return BadRequest();
            }

            var result = await _catalogService.UpdateCatalog(catalog);
            if (result == 0)
            {
                _logger.LogWarning($"Catalog with ID {_id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Catalog with ID {_id} updated successfully");
            return Ok($"Catalog with id {_id} updated successfully");
        }

        [HttpDelete("{_id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteCatalog(Guid _id)
        {
            _logger.LogInformation($"Attempting to delete catalog with ID: {_id}");

            var result = await _catalogService.DeleteCatalog(_id);
            if (result == 0)
            {
                _logger.LogWarning($"Catalog with ID {_id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Catalog with ID {_id} deleted successfully");
            return Ok($"Catalog with ID {_id} deleted successfully");
        }
    }
}