using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Controllers;
using SportsStore.Infrastructure;
using SportsStore.Models;
using Xunit;

namespace SportsStore.Tests {

    public class OrderControllerTests {

        private IPaymentService GetMockPaymentService() {
            var mockPayment = new Mock<IPaymentService>();
            mockPayment.Setup(p => p.CreatePaymentIntent(
                It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new Stripe.PaymentIntent { Id = "pi_test_123", Status = "succeeded" });
            return mockPayment.Object;
        }

        [Fact]
        public void Cannot_Checkout_Empty_Cart() {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Cart cart = new Cart();
            Order order = new Order();
            OrderController target = new OrderController(mock.Object, cart, GetMockPaymentService());

            ViewResult? result = target.Checkout(order).Result as ViewResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Cannot_Checkout_Invalid_ShippingDetails() {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Cart cart = new Cart();
            cart.AddItem(new SportsStore.Models.Product(), 1);
            OrderController target = new OrderController(mock.Object, cart, GetMockPaymentService());
            target.ModelState.AddModelError("error", "error");

            ViewResult? result = target.Checkout(new Order()).Result as ViewResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Can_Checkout_And_Submit_Order() {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Cart cart = new Cart();
            cart.AddItem(new SportsStore.Models.Product(), 1);
            OrderController target = new OrderController(mock.Object, cart, GetMockPaymentService());

            RedirectToPageResult? result =
                target.Checkout(new Order()).Result as RedirectToPageResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Once);
            Assert.Equal("/Completed", result?.PageName);
        }
    }
}