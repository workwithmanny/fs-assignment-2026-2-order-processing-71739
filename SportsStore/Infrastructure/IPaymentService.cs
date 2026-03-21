using Stripe;

namespace SportsStore.Infrastructure
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntent(long amount, string currency = "usd");
    }
}
