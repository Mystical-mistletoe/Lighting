using Lighting.Data;
using Lighting.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lighting
{
    public partial class ProductEditWindow : Window
    {
        private AppDbContext db;
        private Product? existingProduct;
        private ObservableCollection<ProductCharDisplay> charList = new ObservableCollection<ProductCharDisplay>();

        public class ProductCharDisplay
        {
            public int Id { get; set; }
            public int CharacteristicId { get; set; }
            public string CharName { get; set; } = "";
            public string Value { get; set; } = "";
            public int CharacteristicValueId { get; set; }
        }

        public ProductEditWindow(AppDbContext context, Product? product = null)
        {
            InitializeComponent();
            db = context;

            LoadComboBoxes();

            if (product != null)
            {
                existingProduct = product;
                this.Title = "Редактирование товара";
                LoadProduct(product);
            }
            else
            {
                this.Title = "Добавление товара";
            }

            dgCharacteristics.ItemsSource = charList;
        }

        private void LoadComboBoxes()
        {
            cmbCategory.ItemsSource = db.Categories.ToList();
            cmbManufacturer.ItemsSource = db.Manufacturers.ToList();
            cmbNewCharacteristic.ItemsSource = db.Characteristics.ToList();
        }

        private void LoadProduct(Product product)
        {
            txtArticle.Text = product.Article;
            txtName.Text = product.Name;
            cmbCategory.SelectedValue = product.CategoryId;
            cmbManufacturer.SelectedValue = product.ManufacturerId;
            txtPrice.Text = product.Price.ToString();
            txtStockQuantity.Text = product.StockQuantity.ToString();
            txtImageUrl.Text = product.ImageUrl;
            txtDescription.Text = product.Description;

            foreach (var pc in product.ProductCharacteristics)
            {
                var ch = db.Characteristics.Find(pc.CharacteristicId);
                var cv = db.CharacteristicValues.Find(pc.CharacteristicValueId);
                charList.Add(new ProductCharDisplay
                {
                    Id = pc.Id,
                    CharacteristicId = pc.CharacteristicId,
                    CharName = ch?.Name ?? "",
                    Value = cv?.Value ?? "",
                    CharacteristicValueId = pc.CharacteristicValueId
                });
            }
        }
        private void CmbNewCharacteristic_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (cmbNewCharacteristic.SelectedValue == null) return;
            int charId = (int)cmbNewCharacteristic.SelectedValue;
            var values = db.CharacteristicValues
                .Where(cv => cv.CharacteristicId == charId)
                .Select(cv => new { cv.Id, cv.Value })
                .ToList();
            cmbNewCharValue.ItemsSource = values;
            cmbNewCharValue.DisplayMemberPath = "Value";
            cmbNewCharValue.SelectedValuePath = "Id";
            cmbNewCharValue.SelectedIndex = -1;
            cmbNewCharValue.Text = "";
        }

        private void BtnAddChar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbNewCharacteristic.SelectedValue == null)
            {
                MessageBox.Show("Выберите характеристику");
                return;
            }

            string valueText = cmbNewCharValue.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(valueText))
            {
                MessageBox.Show("Введите или выберите значение");
                return;
            }

            int charId = (int)cmbNewCharacteristic.SelectedValue;
            var ch = db.Characteristics.Find(charId);

            if (charList.Any(c => c.CharacteristicId == charId))
            {
                MessageBox.Show("Эта характеристика уже добавлена");
                return;
            }

            var existingValue = db.CharacteristicValues
                .FirstOrDefault(cv => cv.CharacteristicId == charId && cv.Value == valueText);

            if (existingValue == null)
            {
                existingValue = new CharacteristicValue
                {
                    CharacteristicId = charId,
                    Value = valueText
                };
                db.CharacteristicValues.Add(existingValue);
                db.SaveChanges();
            }

            charList.Add(new ProductCharDisplay
            {
                CharacteristicId = charId,
                CharName = ch?.Name ?? "",
                Value = existingValue.Value,
                CharacteristicValueId = existingValue.Id
            });

            cmbNewCharValue.Text = "";
            cmbNewCharValue.ItemsSource = db.CharacteristicValues
                .Where(cv => cv.CharacteristicId == charId)
                .Select(cv => new { cv.Id, cv.Value })
                .ToList();
        }

        private void BtnRemoveChar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ProductCharDisplay pcd)
            {
                charList.Remove(pcd);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArticle.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                cmbCategory.SelectedValue == null ||
                cmbManufacturer.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtStockQuantity.Text))
            {
                MessageBox.Show("Заполните все обязательные поля (отмечены *)");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом");
                return;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным числом");
                return;
            }

            if (existingProduct == null)
            {
                if (db.Products.Any(p => p.Article == txtArticle.Text.Trim()))
                {
                    MessageBox.Show("Товар с таким артикулом уже существует");
                    return;
                }

                var product = new Product
                {
                    Article = txtArticle.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    CategoryId = (int)cmbCategory.SelectedValue,
                    ManufacturerId = (int)cmbManufacturer.SelectedValue,
                    Price = price,
                    StockQuantity = stock,
                    ImageUrl = txtImageUrl.Text?.Trim(),
                    Description = txtDescription.Text?.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();

                foreach (var pc in charList)
                {
                    db.ProductCharacteristics.Add(new ProductCharacteristic
                    {
                        ProductId = product.Id,
                        CharacteristicId = pc.CharacteristicId,
                        CharacteristicValueId = pc.CharacteristicValueId
                    });
                }
                db.SaveChanges();
            }
            else
            {
                if (db.Products.Any(p => p.Article == txtArticle.Text.Trim() && p.Id != existingProduct.Id))
                {
                    MessageBox.Show("Товар с таким артикулом уже существует");
                    return;
                }

                existingProduct.Article = txtArticle.Text.Trim();
                existingProduct.Name = txtName.Text.Trim();
                existingProduct.CategoryId = (int)cmbCategory.SelectedValue;
                existingProduct.ManufacturerId = (int)cmbManufacturer.SelectedValue;
                existingProduct.Price = price;
                existingProduct.StockQuantity = stock;
                existingProduct.ImageUrl = txtImageUrl.Text?.Trim();
                existingProduct.Description = txtDescription.Text?.Trim();
                existingProduct.UpdatedAt = DateTime.Now;

                var existingChars = db.ProductCharacteristics.Where(pc => pc.ProductId == existingProduct.Id).ToList();
                db.ProductCharacteristics.RemoveRange(existingChars);

                foreach (var pc in charList)
                {
                    db.ProductCharacteristics.Add(new ProductCharacteristic
                    {
                        ProductId = existingProduct.Id,
                        CharacteristicId = pc.CharacteristicId,
                        CharacteristicValueId = pc.CharacteristicValueId
                    });
                }

                db.SaveChanges();
            }

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
