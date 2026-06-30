namespace Lighting.Models
{
    public sealed class CharacteristicValue
    {
        public int Id { get; set; }
        public int CharacteristicId { get; set; }
        public string Value { get; set; } = string.Empty;
        public Characteristic Characteristic { get; set; } = null!;
        public ICollection<ProductCharacteristic> ProductCharacteristics { get; set; } = new List<ProductCharacteristic>();

        public Characteristic Characteristic1
        {
            get => default;
            set
            {
            }
        }
    }
}
