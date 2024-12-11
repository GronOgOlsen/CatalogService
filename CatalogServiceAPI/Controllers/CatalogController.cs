using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CatalogServiceAPI.Models;
using CatalogServiceAPI.Interfaces;

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

        [HttpGet("product/{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<ProductDTO>> GetProduct(Guid id)
        {
            _logger.LogInformation($"Getting product with ID: {id}");
            var product = await _catalogService.GetProduct(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpGet("products")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByCategory([FromQuery] ProductCategory? category)
        {
            _logger.LogInformation($"Getting products for category: {category}");
            var products = await _catalogService.GetProductsByCategory(category);
            return Ok(products);
        }

        [HttpPost("product")]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<Guid>> CreateProduct(ProductDTO product)
        {
            _logger.LogInformation($"Creating new product: {product.Title}");
            var id = await _catalogService.AddProduct(product);
            return Ok(id);
        }

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

        [HttpDelete("product/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation($"Deleting product with ID: {id}");
            var result = await _catalogService.DeleteProduct(id);
            if (result == 0) return NotFound();
            return Ok();
        }

        [HttpPut("product/{id}/prepare-auction")]
        [Authorize(Roles = "2")] // Kun admin
        public async Task<IActionResult> PrepareForAuction(Guid id)
        {
            var success = await _catalogService.PrepareForAuction(id);
            if (!success) return NotFound("Product not found or not available for auction");
            return Ok();
        }
    }
}