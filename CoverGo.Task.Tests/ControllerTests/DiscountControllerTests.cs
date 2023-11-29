using Moq;
using T = System.Threading.Tasks.Task;
using CoverGo.Task.Api.Controllers;
using CoverGo.Task.Application;
using CoverGo.Task.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CoverGo.Task.Tests.ControllerTests
{
    [TestClass]
    public class DiscountControllerTests
    {
        private readonly Mock<IProductsWriteRepository> mockProductsWrite;
        private readonly Mock<IDiscountWriteRepository> mockDiscountWrite;
        private readonly Mock<IDiscountQuery> mockDiscountQuery;
        private readonly Mock<IProductsQuery> mockProductsQuery;
        private readonly Mock<ICartQuery> mockCartQuery;
        private readonly Mock<ICartWriteRepository> mockCartWrite;
        private readonly DiscountController controller;

        public DiscountControllerTests()
        {
            mockProductsQuery = new Mock<IProductsQuery>();
            mockDiscountQuery = new Mock<IDiscountQuery>();
            mockProductsWrite = new Mock<IProductsWriteRepository>();
            mockDiscountWrite = new Mock<IDiscountWriteRepository>();
            mockCartQuery = new Mock<ICartQuery>();
            mockCartWrite = new Mock<ICartWriteRepository>();
            controller = new DiscountController(mockDiscountQuery.Object, mockDiscountWrite.Object, mockProductsQuery.Object);
        }

        [TestMethod]
        public async T CreateDiscountRuleTest_ReturnsOk()
        {
            var rule = new DiscountRule
            {
                productName = "t-shirt",
                forEvery = 3
            };
            mockProductsQuery.Setup(x => x.GetByName("t-shirt", It.IsAny<CancellationToken>())).ReturnsAsync(new Product
            {
                Name = "t-shirt",
                Price = 10,
                Currency = "HKD"
            });
            mockDiscountWrite.Setup(x => x.AddDiscountRule(rule, It.IsAny<CancellationToken>())).ReturnsAsync(rule);

            var result = await controller.AddRule(rule);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(rule, okResult.Value);
            mockDiscountWrite.Verify(x => x.AddDiscountRule(rule, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async T CreateDiscountRuleWithMissingProductTest_ReturnsError()
        {
            // Arrange
            var rule = new DiscountRule
            {
                productName = "not-existing",
                forEvery = 3
            };
            string expectedError = "Product with name not-existing not found.";
            mockProductsQuery
                .Setup(x => x.GetByName("not-existing", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.AddRule(rule);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notfound = (NotFoundObjectResult)result;
            Assert.AreEqual(expectedError, notfound.Value);

            mockDiscountWrite
                .Verify(x => x.AddDiscountRule(rule, It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async T CreateDiscountRuleWithNonPositiveForEveryTest_ReturnsError()
        {
            // Arrange
            var rule = new DiscountRule
            {
                productName = "t-shirt",
                forEvery = -1
            };

            mockProductsQuery
                .Setup(x => x.GetByName("t-shirt", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product
                {
                    Name = "t-shirt",
                    Price = 10,
                    Currency = "HKD"
                });

            // Act
            var result = await controller.AddRule(rule);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badResult = (BadRequestObjectResult)result;
            Assert.AreEqual("forEvery in rule should be a positive number", badResult.Value);

            mockDiscountWrite
                .Verify(x => x.AddDiscountRule(rule, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
