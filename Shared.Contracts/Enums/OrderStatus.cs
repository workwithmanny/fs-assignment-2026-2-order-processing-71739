namespace Shared.Contracts.Enums;

public enum OrderStatus
{
    Submitted,
    InventoryPending,
    InventoryConfirmed,
    InventoryFailed,
    PaymentPending,
    PaymentApproved,
    PaymentFailed,
    ShippingPending,
    ShippingCreated,
    Completed,
    Failed
}
