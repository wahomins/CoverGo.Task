using Moq;
using T = System.Threading.Tasks.Task;
using CoverGo.Task.Api.Controllers;
using CoverGo.Task.Application;
using CoverGo.Task.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CoverGo.Task.Tests.ControllerTests
{
    [TestClass]
    public class CartControllerTests
    {
        private readonly Mock<ICartQuery> mockCartQuery;
        private readonly Mock<ICartWriteRepository> mockCartWrite;
        private readonly Mock<IProductsWriteRepository> mockProductsWrite;
        private readonly Mock<IProductsQuery> mockProductsQuery;
        private readonly Mock<IDiscountQuery> mockDiscountQuery;
        private readonly CartController controller;

        public CartControllerTests()
        {
            mockCartQuery = new Mock<ICartQuery>();
            mockCartWrite = new Mock<ICartWriteRepository>();
            mockProductsWrite = new Mock<IProductsWriteRepository>();
            mockProductsQuery = new Mock<IProductsQuery>();
            mockDiscountQuery = new Mock<IDiscountQuery>();
            controller = new CartController(mockCartQuery.Object, mockCartWrite.Object, mockProductsQuery.Object, mockDiscountQuery.Object);
        }

        [TestMethod]
        public async T AddToCart_ProductExists_ReturnsOk()
        {
            // Arrange
            var cartRequest = new CartRequestObject { customerId = "customer1", product = "tennis ball", quantity = 1 };
            var product = new Product { Name = "tennis ball", Price = 5, Currency = "HKD" };
            var cartItem = new CartItem { product = product, quantity = cartRequest.quantity };

            mockProductsQuery.Setup(pq => pq.GetByName("tennis ball", It.IsAny<CancellationToken>())).ReturnsAsync(product);
            mockCartWrite.Setup(cw => cw.AddToCart(It.IsAny<CartItem>(), "customer1", It.IsAny<CancellationToken>())).ReturnsAsync(new ShoppingCart { CustomerId = "customer1", Items = new List<CartItem> { cartItem } });

            // Act
            var result = await controller.AddToCart(cartRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            var shoppingCart = (ShoppingCart)okResult.Value;
            Assert.AreEqual("customer1", shoppingCart.CustomerId);
            Assert.AreEqual("tennis ball", shoppingCart.Items[0].product.Name);
            mockProductsQuery.Verify(pq => pq.GetByName("tennis ball", It.IsAny<CancellationToken>()), Times.Once);
            mockCartWrite.Verify(cw => cw.AddToCart(It.IsAny<CartItem>(), "customer1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async T AddDifferentItemsToCart_ProductsExist_ReturnsOk()
        {
            var cartRequests = new List<CartRequestObject>
            {
                new CartRequestObject { customerId = "customer1", product = "tennis ball", quantity = 1 },
                new CartRequestObject { customerId = "customer1", product = "tennis racket", quantity = 1 }
            };

            var products = new List<Product>
            {
                new Product { Name = "tennis ball", Price = 5, Currency = "HKD" },
                new Product { Name = "tennis racket", Price = 20, Currency = "HKD" }
            };

            var cartItems = new List<CartItem>
            {
                new CartItem { product = products[0], quantity = cartRequests[0].quantity },
                new CartItem { product = products[1], quantity = cartRequests[1].quantity }
            };

            var shoppingCart = new ShoppingCart { CustomerId = "customer1", Items = cartItems };

            foreach (var request in cartRequests)
            {
                var product = products.First(p => p.Name == request.product);
                mockProductsWrite.Setup(pw => pw.AddProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(product);
                mockProductsQuery.Setup(pq => pq.GetByName(request.product, It.IsAny<CancellationToken>())).ReturnsAsync(product);
                mockCartWrite.Setup(cw => cw.AddToCart(It.IsAny<CartItem>(), "customer1", It.IsAny<CancellationToken>())).ReturnsAsync(shoppingCart);
            }

            foreach (var request in cartRequests)
            {
                var result = await controller.AddToCart(request);

                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var okResult = (OkObjectResult)result;
                Assert.IsNotNull(okResult);
                Assert.IsNotNull(okResult.Value);
                var returnedCart = (ShoppingCart)okResult.Value;
                Assert.AreEqual("customer1", returnedCart.CustomerId);
                Assert.IsTrue(returnedCart.Items.Any(i => i.product.Name == request.product));
            }
        }
        
        [TestMethod]
        public async T AddNonExistingProductToCart_ReturnsError()
        {
            // Arrange
            var cartRequest = new CartRequestObject { customerId = "customer1", product = "non-existing product", quantity = 1 };
            string expectedError = "Product with name non-existing product not found.";

            // Setup the product query to return null for a non-existing product
            mockProductsQuery.Setup(pq => pq.GetByName("non-existing product", It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await controller.AddToCart(cartRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult)); // Check that a NotFoundObjectResult is returned
            var notfound = (NotFoundObjectResult)result;
            Assert.AreEqual(expectedError, notfound.Value);
            mockCartWrite.Verify(x => x.AddToCart(It.IsAny<CartItem>(), "customer1", It.IsAny<CancellationToken>()), Times.Never); // Verify that AddToCart was never called
        }
        [TestMethod]
        public async T CalculateTotalPriceInCart_ReturnsCorrectTotal()
        {
            // Arrange
            var customerId = "customer1";
            var discountRules = new List<DiscountRule>(); // Assuming no discount rules for this test
            var cartItems = new List<CartItem>
            {
                new CartItem { product = new Product { Name = "tennis ball", Price = 5, Currency = "HKD" }, quantity = 1 },
                new CartItem { product = new Product { Name = "tennis racket", Price = 20, Currency = "HKD" }, quantity = 2 },
                new CartItem { product = new Product { Name = "t-shirt", Price = 10, Currency = "HKD" }, quantity = 3 }
            };
            var shoppingCart = new ShoppingCart { CustomerId = customerId, Items = cartItems };
            var expectedTotalPrice = cartItems.Sum(item => item.product.Price * item.quantity);
            var expectedTotal = new Dictionary<string, decimal> { { "totalPrice", expectedTotalPrice }, { "totalDiscount", 0 } };

            mockCartQuery.Setup(cq => cq.GetByCustomerId(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(shoppingCart);
            mockDiscountQuery.Setup(dq => dq.ExecuteAsync()).ReturnsAsync(discountRules);

            // Act
            var result = await controller.GetCartTotals(customerId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultValue = (Dictionary<string, decimal>)okResult.Value;
            Assert.AreEqual(expectedTotal["totalPrice"], resultValue["totalPrice"]);
            Assert.AreEqual(expectedTotal["totalDiscount"], resultValue["totalDiscount"]);
        }
        [TestMethod]
        public async T WithDiscount_GetCartTotals_ReturnsCorrectTotal()
        {
            // Arrange
            var customerId = "customer1";
            var discountRules = new List<DiscountRule>
            {
                new DiscountRule { productName = "t-shirt", forEvery = 3 }
            };
            var cartItems = new List<CartItem>
            {
                new CartItem { product = new Product { Name = "tennis ball", Price = 5, Currency = "HKD" }, quantity = 1 },
                new CartItem { product = new Product { Name = "tennis racket", Price = 20, Currency = "HKD" }, quantity = 2 },
                new CartItem { product = new Product { Name = "t-shirt", Price = 10, Currency = "HKD" }, quantity = 3 }
            };
            var shoppingCart = new ShoppingCart { CustomerId = customerId, Items = cartItems };
            var totalDiscount = cartItems
                .Where(item => discountRules.Any(rule => rule.productName == item.product.Name && item.quantity >= rule.forEvery))
                .Sum(item => item.product.Price * (item.quantity / discountRules.First(rule => rule.productName == item.product.Name).forEvery));
            var expectedTotalPrice = cartItems.Sum(item => item.product.Price * item.quantity) - totalDiscount;
            var expectedTotal = new Dictionary<string, decimal> { { "totalPrice", expectedTotalPrice }, { "totalDiscount", totalDiscount } };

            mockCartQuery.Setup(cq => cq.GetByCustomerId(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(shoppingCart);
            mockDiscountQuery.Setup(dq => dq.ExecuteAsync()).ReturnsAsync(discountRules);

            // Act
            var result = await controller.GetCartTotals(customerId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultValue = (Dictionary<string, decimal>)okResult.Value;
            Assert.AreEqual(expectedTotal["totalPrice"], resultValue["totalPrice"]);
            Assert.AreEqual(expectedTotal["totalDiscount"], resultValue["totalDiscount"]);
        }
    }
}
