using Lighting.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace Lighting
{
    public partial class ProductViewWindow : Window
    {
        private AppDbContext db = new AppDbContext();

        public ProductViewWindow(int productId)
        {
            InitializeComponent();

            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCharacteristics)
                    .ThenInclude(pc => pc.Characteristic)
                .Include(p => p.ProductCharacteristics)
                    .ThenInclude(pc => pc.CharacteristicValue)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                MessageBox.Show("Товар не найден");
                this.Close();
                return;
            }

            txtArticle.Text = product.Article;
            txtName.Text = product.Name;
            txtCategory.Text = product.Category?.Name ?? "";
            txtManufacturer.Text = product.Manufacturer?.Name ?? "";
            txtPrice.Text = product.Price.ToString("F2") + " ₽";
            txtStock.Text = product.StockQuantity.ToString();
            txtDescription.Text = product.Description;

            var chars = product.ProductCharacteristics
                .Select(pc => new
                {
                    CharName = pc.Characteristic?.Name ?? "",
                    Value = pc.CharacteristicValue?.Value ?? ""
                })
                .ToList();

            dgViewCharacteristics.ItemsSource = chars;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
