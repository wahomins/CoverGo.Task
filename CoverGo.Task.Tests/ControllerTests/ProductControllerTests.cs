using Moq;
using T = System.Threading.Tasks.Task;
using CoverGo.Task.Api.Controllers;
using CoverGo.Task.Application;
using CoverGo.Task.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CoverGo.Task.Tests.ControllerTests
{
    [TestClass]
    public class ProductControllerTests
    {
        private readonly Mock<IProductsWriteRepository> mockProductsWrite;
        private readonly Mock<IProductsQuery> mockProductsQuery;
        private readonly ProductsController controller;

        public ProductControllerTests()
        {
            mockProductsQuery = new Mock<IProductsQuery>();
            mockProductsWrite = new Mock<IProductsWriteRepository>();
            controller = new ProductsController(mockProductsQuery.Object, mockProductsWrite.Object);
        }

        [TestMethod]
        public async T AddFirstProduct_ReturnsOk()
        {
            var newProduct = new Product { Name = "tennis ball", Price = 5, Currency = "HKD" };

            mockProductsQuery.Setup(pq => pq.ExecuteAsync()).ReturnsAsync(new List<Product>());
            mockProductsWrite.Setup(pw => pw.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(newProduct);

            var result = await controller.AddProduct(newProduct);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

        }

        [TestMethod]
        public async T AddSecondProduct_RwturnsOk()
        {
            var products = new List<Product>();
            var firstProduct = new Product { Name = "tennis ball", Price = 5, Currency = "HKD" };
            var secondProduct = new Product { Name = "tennis racket", Price = 20, Currency = "HKD" };

            mockProductsQuery.Setup(pq => pq.ExecuteAsync()).ReturnsAsync(() => products);
            mockProductsWrite.Setup(pw => pw.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                 .Callback<Product, CancellationToken>((p, c) => products.Add(p))
                 .ReturnsAsync((Product p, CancellationToken c) => p);


            await controller.AddProduct(firstProduct);
            await controller.AddProduct(secondProduct);
            var result = await controller.GetAll();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedProducts = okResult.Value as List<Product>;
            Assert.IsNotNull(returnedProducts);
            Assert.AreEqual(2, returnedProducts.Count);
            Assert.AreEqual(firstProduct, returnedProducts[0]);
            Assert.AreEqual(secondProduct, returnedProducts[1]);
        }

        [TestMethod]
        public async T AddProductWithEmptyName_ReturnsError()
        {
            // Arrange
            var products = new List<Product>();
            var newProduct = new Product { Name = "", Price = 5, Currency = "HKD" };
            string expectedError = "Product name cannot be empty.";

            mockProductsQuery.Setup(pq => pq.ExecuteAsync()).ReturnsAsync(() => products);
            mockProductsWrite.Setup(pw => pw.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(newProduct);

            // Act
            var result = await controller.AddProduct(newProduct);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(expectedError, badRequestResult.Value);
            mockProductsWrite.Verify(x => x.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async T AddProductWithNegativePrice_ReturnsError()
        {
            var products = new List<Product>();
            var newProduct = new Product { Name = "tennis ball", Price = -5, Currency = "HKD" };
            string expectedError = "Price cannot be negative.";

            mockProductsQuery.Setup(pq => pq.ExecuteAsync()).ReturnsAsync(() => products);
            mockProductsWrite.Setup(pw => pw.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(newProduct);

            var result = await controller.AddProduct(newProduct);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(expectedError, badRequestResult.Value);
            mockProductsWrite.Verify(x => x.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
