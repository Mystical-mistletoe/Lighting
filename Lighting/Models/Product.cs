namespace Lighting.Models
{
    public sealed class Product
    {
        public int Id { get; set; }
        public string Article { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int ManufacturerId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Category Category { get; set; } = null!;
        public Manufacturer Manufacturer { get; set; } = null!;
        public ICollection<ProductCharacteristic> ProductCharacteristics { get; set; } = new List<ProductCharacteristic>();
    }
}
