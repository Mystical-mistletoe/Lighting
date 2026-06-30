namespace Lighting.Models
{
    public sealed class Characteristic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public ICollection<ProductCharacteristic> ProductCharacteristics { get; set; } = new List<ProductCharacteristic>();
        public ICollection<CharacteristicValue> CharacteristicValues { get; set; } = new List<CharacteristicValue>();
    }
}
