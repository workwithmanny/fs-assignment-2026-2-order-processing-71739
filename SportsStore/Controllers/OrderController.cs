using Microsoft.AspNetCore.Mvc;
using Serilog;
using SportsStore.Infrastructure;
using SportsStore.Models;

namespace SportsStore.Controllers
{

    public class OrderController : Controller
    {
        private IOrderRepository repository;
        private Cart cart;
        private IPaymentService paymentService;

        public OrderController(IOrderRepository repoService, Cart cartService, IPaymentService stripeService)
        {
            repository = repoService;
            cart = cartService;
            paymentService = stripeService;
        }

        public ViewResult Checkout() => View(new Order());

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            if (cart.Lines.Count() == 0)
            {
                Log.Warning("Checkout attempted with empty cart - Adeniyi Emmanuel");
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    long amount = (long)(cart.Lines.Sum(l => l.Product.Price * l.Quantity) * 100);
                    Log.Information("Creating Stripe PaymentIntent for amount {Amount} - Adeniyi Emmanuel", amount);

                    var paymentIntent = await paymentService.CreatePaymentIntent(amount, "usd");

                    Log.Information("PaymentIntent created: {PaymentIntentId} Status: {Status} - Adeniyi Emmanuel",
                        paymentIntent.Id, paymentIntent.Status);

                    order.Lines = cart.Lines.ToArray();
                    order.PaymentIntentId = paymentIntent.Id;
                    order.PaymentStatus = paymentIntent.Status;

                    repository.SaveOrder(order);
                    Log.Information("Order {OrderID} saved for {Name} - Adeniyi Emmanuel",
                        order.OrderID, order.Name);

                    cart.Clear();
                    return RedirectToPage("/Completed", new { orderId = order.OrderID });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Payment failed for order - Adeniyi Emmanuel");
                    ModelState.AddModelError("", "Payment failed: " + ex.Message);
                    return View(order);
                }
            }
            else
            {
                return View();
            }
        }

        public IActionResult CancelPayment()
        {
            Log.Warning("Payment cancelled by user - Adeniyi Emmanuel");
            return RedirectToAction("Index", "Cart");
        }
    }
}