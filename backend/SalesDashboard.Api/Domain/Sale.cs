namespace SalesDashboard.Api.Domain;

public enum SaleStatus { Paid, Cancelled, Refunded }

public class Sale
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Момент продажи, всегда UTC (timestamptz).</summary>
    public DateTime SoldAt { get; set; }
    public SaleStatus Status { get; set; }

    // Денормализованные итоги: считаются только через AddItem
    public decimal TotalAmount { get; private set; }
    public decimal TotalCost { get; private set; }

    public List<SaleItem> Items { get; private set; } = [];

    public void AddItem(Product product, int quantity, decimal unitPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Items.Add(new SaleItem
        {
            Product = product,
            Quantity = quantity,
            UnitPrice = unitPrice,
            UnitCost = product.UnitCost   // snapshot себестоимости на момент продажи
        });

        TotalAmount += quantity * unitPrice;
        TotalCost += quantity * product.UnitCost;
    }
}