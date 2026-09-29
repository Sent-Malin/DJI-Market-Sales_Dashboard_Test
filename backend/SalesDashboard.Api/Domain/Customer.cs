namespace SalesDashboard.Api.Domain;

public enum CustomerSegment { Smb, MidMarket, Enterprise }

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }      // контактное лицо
    public required string Company { get; set; }
    public CustomerSegment Segment { get; set; }

    public List<Sale> Sales { get; set; } = [];
}