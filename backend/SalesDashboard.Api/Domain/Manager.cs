namespace SalesDashboard.Api.Domain;

public class Manager
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Team { get; set; }
    public required string Position { get; set; }
    public string? AvatarUrl { get; set; }        // null → на фронте показываем инициалы
    public bool IsActive { get; set; } = true;
    public DateOnly HiredAt { get; set; }

    public List<Sale> Sales { get; set; } = [];
}
