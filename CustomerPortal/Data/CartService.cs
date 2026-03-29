namespace CustomerPortal.Data;

public class CartService
{
    private List<CartItemDto> _items = new();

    public IReadOnlyList<CartItemDto> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(i => i.UnitPrice * i.Quantity);

    public int ItemCount => _items.Sum(i => i.Quantity);

    public event Action? OnChange;

    public void AddItem(ProductDto product, int quantity = 1)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItemDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price
            });
        }
        OnChange?.Invoke();
    }

    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            OnChange?.Invoke();
        }
    }

    public void Clear()
    {
        _items.Clear();
        OnChange?.Invoke();
    }
}