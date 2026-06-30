namespace Lighting.Models
{
    public class ProductCharacteristic
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CharacteristicId { get; set; }
        public int CharacteristicValueId { get; set; }

        public Product Product { get; set; } = null!;
        public Characteristic Characteristic { get; set; } = null!;
        public CharacteristicValue CharacteristicValue { get; set; } = null!;
    }
}
