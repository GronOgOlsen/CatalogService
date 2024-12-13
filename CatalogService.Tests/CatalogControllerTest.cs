using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CatalogServiceAPI.Controllers;
using CatalogServiceAPI.Interfaces;
using CatalogServiceAPI.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogService.Tests.Controllers
{
    [TestClass]
    public class CatalogControllerTests
    {
        private Mock<ICatalogInterface> _catalogServiceMock; // Mock for service laget (ICatalogInterface)
        private Mock<ILogger<CatalogController>> _loggerMock; // Mock for logging (ILogger)
        private CatalogController _catalogController; // Controlleren vi tester

        [TestInitialize]
        public void Setup()
        {
            // Initialiser mocks og controller før hver test
            _catalogServiceMock = new Mock<ICatalogInterface>();
            _loggerMock = new Mock<ILogger<CatalogController>>();
            _catalogController = new CatalogController(_catalogServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task CreateProduct_ReturnsOkWithProductId()
        {
            // Arrange: Opretter et nyt produkt og et mocket produkt-ID
            var newProduct = new ProductDTO { Title = "Smartwatch", CustomerPrice = 300.00m };
            var productId = Guid.NewGuid();

            // Mock opførsel for AddProduct metoden
            _catalogServiceMock.Setup(service => service.AddProduct(newProduct))
                .ReturnsAsync(productId);

            // Act: Kalder CreateProduct på controlleren
            var result = await _catalogController.CreateProduct(newProduct);

            // Assert: Verificerer at resultatet er en HTTP 200 med korrekt produkt-ID
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult, "Resultatet bør være af typen OkObjectResult.");
            Assert.AreEqual(200, okResult.StatusCode, "Statuskoden bør være 200.");
            Assert.AreEqual(productId, okResult.Value, "Det returnerede produkt-ID stemmer ikke.");
        }

        [TestMethod]
        public async Task GetProduct_ReturnsNotFoundWhenProductDoesNotExist()
        {
            // Arrange: Brug en produkt-ID, der ikke findes
            var productId = Guid.NewGuid();

            // Mock opførsel for GetProduct metoden til at returnere null
            _catalogServiceMock.Setup(service => service.GetProduct(productId))
                .ReturnsAsync((ProductDTO)null);

            // Act: Kalder GetProduct på controlleren
            var result = await _catalogController.GetProduct(productId);

            // Assert: Verificerer at resultatet er en HTTP 404
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult, "Resultatet bør være af typen NotFoundResult.");
            Assert.AreEqual(404, notFoundResult.StatusCode, "Statuskoden bør være 404.");
        }

        [TestMethod]
        public async Task GetAllProducts_ReturnsOkWithProducts()
        {
            // Arrange: Opret en liste af produkter
            var products = new List<ProductDTO>
            {
                new ProductDTO { ProductId = Guid.NewGuid(), Title = "Gaming Laptop", CustomerPrice = 1500.00m },
                new ProductDTO { ProductId = Guid.NewGuid(), Title = "Smartphone", CustomerPrice = 800.00m }
            };

            // Mock opførsel for GetAllProducts metoden
            _catalogServiceMock.Setup(service => service.GetAllProducts())
                .ReturnsAsync(products);

            // Act: Kalder GetAllProducts på controlleren
            var result = await _catalogController.GetAllProducts();

            // Assert: Verificerer at resultatet er en HTTP 200 med produktlisten
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult, "Resultatet bør være af typen OkObjectResult.");
            Assert.AreEqual(200, okResult.StatusCode, "Statuskoden bør være 200.");
            var returnedProducts = okResult.Value as List<ProductDTO>;
            Assert.AreEqual(2, returnedProducts.Count, "Antallet af returnerede produkter stemmer ikke.");
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsOkWhenProductUpdated()
        {
            // Arrange: Opret et produkt og mock opdatering
            var productId = Guid.NewGuid();
            var updatedProduct = new ProductDTO { ProductId = productId, Title = "Updated Laptop", CustomerPrice = 1700.00m };

            _catalogServiceMock.Setup(service => service.UpdateProduct(updatedProduct))
                .ReturnsAsync(1); // Simulerer en opdatering

            // Act: Kalder UpdateProduct på controlleren
            var result = await _catalogController.UpdateProduct(productId, updatedProduct);

            // Assert: Verificerer at resultatet er en HTTP 200
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult, "Resultatet bør være af typen OkResult.");
            Assert.AreEqual(200, okResult.StatusCode, "Statuskoden bør være 200.");
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsBadRequestWhenIdsMismatch()
        {
            // Arrange: Brug mismatch mellem produktets ID og route ID
            var productId = Guid.NewGuid();
            var updatedProduct = new ProductDTO { ProductId = Guid.NewGuid(), Title = "Updated Laptop", CustomerPrice = 1700.00m };

            // Act: Kalder UpdateProduct med mismatch
            var result = await _catalogController.UpdateProduct(productId, updatedProduct);

            // Assert: Verificerer at resultatet er en HTTP 400 (BadRequest)
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult, "Resultatet bør være af typen BadRequestResult.");
            Assert.AreEqual(400, badRequestResult.StatusCode, "Statuskoden bør være 400.");
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsNotFoundWhenProductDoesNotExist()
        {
            // Arrange: Simuler at produktet ikke findes
            var productId = Guid.NewGuid();
            var updatedProduct = new ProductDTO { ProductId = productId, Title = "Updated Laptop", CustomerPrice = 1700.00m };

            _catalogServiceMock.Setup(service => service.UpdateProduct(updatedProduct))
                .ReturnsAsync(0); // Ingen opdateringer skete

            // Act: Kalder UpdateProduct
            var result = await _catalogController.UpdateProduct(productId, updatedProduct);

            // Assert: Verificerer at resultatet er en HTTP 404 (NotFound)
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult, "Resultatet bør være af typen NotFoundResult.");
            Assert.AreEqual(404, notFoundResult.StatusCode, "Statuskoden bør være 404.");
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsOkWhenProductDeleted()
        {
            // Arrange: Simuler en sletning
            var productId = Guid.NewGuid();

            _catalogServiceMock.Setup(service => service.DeleteProduct(productId))
                .ReturnsAsync(1); // Én sletning skete

            // Act: Kalder DeleteProduct på controlleren
            var result = await _catalogController.DeleteProduct(productId);

            // Assert: Verificerer at resultatet er en HTTP 200
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult, "Resultatet bør være af typen OkResult.");
            Assert.AreEqual(200, okResult.StatusCode, "Statuskoden bør være 200.");
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNotFoundWhenProductDoesNotExist()
        {
            // Arrange: Simuler at produktet ikke findes
            var productId = Guid.NewGuid();

            _catalogServiceMock.Setup(service => service.DeleteProduct(productId))
                .ReturnsAsync(0); // Ingen sletning skete

            // Act: Kalder DeleteProduct
            var result = await _catalogController.DeleteProduct(productId);

            // Assert: Verificerer at resultatet er en HTTP 404 (NotFound)
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult, "Resultatet bør være af typen NotFoundResult.");
            Assert.AreEqual(404, notFoundResult.StatusCode, "Statuskoden bør være 404.");
        }
    }
}
