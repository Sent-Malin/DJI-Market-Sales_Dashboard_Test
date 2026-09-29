namespace SalesDashboard.Api.Domain;

public class Product
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal ListPrice { get; set; }   // текущая прайсовая цена
    public decimal UnitCost { get; set; }    // текущая себестоимость
    public bool IsActive { get; set; } = true;
}