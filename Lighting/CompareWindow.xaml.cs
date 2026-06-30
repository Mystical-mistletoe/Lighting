using Lighting.Data;
using Lighting.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lighting
{
    public partial class CompareWindow : Window
    {
        private AppDbContext db = new AppDbContext();
        private List<int> productIds;
        private List<CompareRow> allRows = new List<CompareRow>();

        public class CompareRow
        {
            public string PropertyName { get; set; } = "";
            public List<string> Values { get; set; } = new List<string>();
            public bool HasDifferences { get; set; }
        }

        public CompareWindow(List<int> ids)
        {
            InitializeComponent();
            productIds = ids;
            LoadComparisonData();
        }

        private void LoadComparisonData()
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCharacteristics)
                    .ThenInclude(pc => pc.Characteristic)
                .Include(p => p.ProductCharacteristics)
                    .ThenInclude(pc => pc.CharacteristicValue)
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            allRows.Clear();

            void AddRow(string name, Func<Product, string> getVal)
            {
                var vals = products.Select(p => getVal(p)).ToList();
                allRows.Add(new CompareRow
                {
                    PropertyName = name,
                    Values = vals,
                    HasDifferences = vals.Distinct().Count() > 1
                });
            }

            AddRow("Артикул", p => p.Article);
            AddRow("Наименование", p => p.Name);
            AddRow("Категория", p => p.Category?.Name ?? "");
            AddRow("Производитель", p => p.Manufacturer?.Name ?? "");
            AddRow("Цена", p => p.Price.ToString("F2") + " ₽");
            AddRow("Остаток", p => p.StockQuantity.ToString());
            AddRow("Описание", p => p.Description ?? "");

            var allCharNames = products
                .SelectMany(p => p.ProductCharacteristics.Select(pc => pc.Characteristic?.Name ?? ""))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (var charName in allCharNames)
            {
                var vals = products.Select(p =>
                {
                    var pc = p.ProductCharacteristics.FirstOrDefault(c => c.Characteristic?.Name == charName);
                    return pc?.CharacteristicValue?.Value ?? "-";
                }).ToList();

                allRows.Add(new CompareRow
                {
                    PropertyName = charName,
                    Values = vals,
                    HasDifferences = vals.Distinct().Count() > 1
                });
            }

            BuildComparisonTable();
        }

        private void BuildComparisonTable()
        {
            dgCompare.Columns.Clear();

            var products = db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            var firstCol = new DataGridTextColumn
            {
                Header = "Характеристика",
                Binding = new Binding("PropertyName"),
                Width = 200,
                FontWeight = FontWeights.Bold
            };
            dgCompare.Columns.Add(firstCol);

            for (int i = 0; i < productIds.Count; i++)
            {
                var colIndex = i;
                var product = products.FirstOrDefault(p => p.Id == productIds[i]);
                var col = new DataGridTextColumn
                {
                    Header = product?.Name ?? $"Товар {i + 1}",
                    Binding = new Binding($"Values[{i}]"),
                    Width = 200,
                    FontSize = 12
                };
                dgCompare.Columns.Add(col);
            }

            ApplyMode();
        }

        private void ApplyMode()
        {
            if (rbDifferences == null) return;
            if (rbDifferences.IsChecked == true)
            {
                dgCompare.ItemsSource = new ObservableCollection<CompareRow>(
                    allRows.Where(r => r.HasDifferences).ToList());
            }
            else
            {
                dgCompare.ItemsSource = new ObservableCollection<CompareRow>(allRows);
            }
        }

        private void OnModeChanged(object sender, RoutedEventArgs e)
        {
            ApplyMode();
        }
    }
}
