using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CatalogServiceAPI.Models;
using CatalogServiceAPI.Interfaces;
using System.Diagnostics;

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
        }

        // Henter alle produkter (kun tilgængelig for administratorer)
        [HttpGet("products")]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
        {
            _logger.LogInformation("Getting all products");
            var products = await _catalogService.GetAllProducts();
            return Ok(products);
        }

        // Henter et specifikt produkt (tilgængelig for både brugere og administratorer)
        [HttpGet("product/{id}")]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<ProductDTO>> GetProduct(Guid id)
        {
            _logger.LogInformation($"Getting product with ID: {id}");
            var product = await _catalogService.GetProduct(id);
            if (product == null) return NotFound();
            return product;
        }

        // Henter alle tilgængelige produkter (tilgængelig for både brugere og administratorer)
        [HttpGet("products/available")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAvailableProducts()
        {
            _logger.LogInformation("Getting available products");
            var products = await _catalogService.GetAvailableProducts();
            return Ok(products);
        }

        // Tjekker om et produkt er tilgængeligt (kaldt af AuctionService, derfor ingen [Authorize])
        [HttpGet("product/{id}/available")]
        public async Task<ActionResult<ProductDTO>> GetAvailableProduct(Guid id)
        {
            var product = await _catalogService.GetProduct(id);
            if (product == null || product.Status != ProductStatus.Available)
            {
                return NotFound("Product not found or not available.");
            }
            return Ok(product);
        }

        // Opretter et nyt produkt (tilgængelig for både brugere og administratorer)
        [HttpPost("product")]
        [Authorize(Roles = "1, 2")]
        public async Task<ActionResult<Guid>> CreateProduct(ProductDTO product)
        {
            _logger.LogInformation($"Creating new product: {product.Title}");
            var id = await _catalogService.AddProduct(product);
            return Ok(id);
        }

        // Opdaterer et produkt (kun tilgængelig for administratorer)
        [HttpPut("product/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductDTO product)
        {
            if (id != product.ProductId) return BadRequest();

            _logger.LogInformation($"Updating product with ID: {id}");
            var result = await _catalogService.UpdateProduct(product);
            if (result == 0) return NotFound();
            return Ok();
        }

        // Forbereder et produkt til auktion (kaldt af AuctionService, derfor ingen [Authorize])
        [HttpPut("product/{id}/prepare-auction")]
        public async Task<IActionResult> PrepareForAuction(Guid id)
        {
            var success = await _catalogService.PrepareForAuction(id);
            if (!success)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound($"Product with ID: {id} not found.");
            }
            return Ok($"Product with ID: {id} has been set to available.");
        }

        // Sætter et produkt som "i auktion" (kaldt af AuctionService, derfor ingen [Authorize])
        [HttpPut("product/{id}/set-in-auction")]
        public async Task<IActionResult> SetInAuction(Guid id, [FromBody] Guid auctionId)
        {
            var success = await _catalogService.SetInAuction(id, auctionId);
            if (!success) return NotFound("Product not found or not available for auction");
            return Ok();
        }

        // Sætter et produkt som "solgt" (kaldt af AuctionService, derfor ingen [Authorize])
        [HttpPut("product/{id}/set-sold")]
        public async Task<IActionResult> SetSold(Guid id)
        {
            var success = await _catalogService.SetSold(id);
            if (!success) return NotFound("Product not found or not available for auction");
            return Ok();
        }

        // Sætter et produkt som "mislykket auktion" (kaldt af AuctionService, derfor ingen [Authorize])
        [HttpPut("product/{id}/set-failed-in-auction")]
        public async Task<IActionResult> SetFailedInAuction(Guid id)
        {
            var success = await _catalogService.SetFailedInAuction(id);
            if (!success) return NotFound("Product not found or not available for auction");
            return Ok();
        }

        // Sletter et produkt (kun tilgængelig for administratorer)
        [HttpDelete("product/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation($"Deleting product with ID: {id}");
            var result = await _catalogService.DeleteProduct(id);
            if (result == 0) return NotFound();
            return Ok();
        }

        [HttpGet("version")]
        public async Task<Dictionary<string, string>> GetVersion()
        {
            var properties = new Dictionary<string, string>();
            var assembly = typeof(Program).Assembly;
            properties.Add("service", "CatalogService");
            var ver = FileVersionInfo.GetVersionInfo(typeof(Program)
            .Assembly.Location).ProductVersion;
            properties.Add("version", ver!);
            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
                var ipa = ips.First().MapToIPv4().ToString();
                properties.Add("hosted-at-address", ipa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                properties.Add("hosted-at-address", "Could not resolve IP-address");
            }
            return properties;
        }
    }
}
