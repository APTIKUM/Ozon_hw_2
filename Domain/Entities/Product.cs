using Domain.Enums;

namespace Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Weight { get; set; }
    public ProductType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int WarehouseNumber { get; set; }
}