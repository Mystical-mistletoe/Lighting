using Lighting.Data;
using Lighting.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Lighting
{
    public partial class MainWindow : Window
    {
        private AppDbContext db = new AppDbContext();
        private List<SearchCondition> searchConditions = new List<SearchCondition>();
        private List<ProductDisplay>? currentProductSource;

        public MainWindow()
        {
            InitializeComponent();
            db.Database.EnsureCreated();
            DataSeeder.Seed(db);

            string role = CurrentAccount.CurAccount?.Role ?? "user";
            string login = CurrentAccount.CurAccount?.Login ?? "гость";
            txtUserInfo.Text = $"Пользователь: {login} | Роль: {role}";
            this.Title = $"Освещение ({role})";

            if (role == "user")
            {
                tabUsers.Visibility = Visibility.Collapsed;
                btnAddProduct.Visibility = Visibility.Collapsed;
                btnEditProduct.Visibility = Visibility.Collapsed;
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                btnAddCategory.Visibility = Visibility.Collapsed;
                btnUpdateCategory.Visibility = Visibility.Collapsed;
                btnDeleteCategory.Visibility = Visibility.Collapsed;
                btnAddManufacturer.Visibility = Visibility.Collapsed;
                btnUpdateManufacturer.Visibility = Visibility.Collapsed;
                btnDeleteManufacturer.Visibility = Visibility.Collapsed;
                btnAddCharacteristic.Visibility = Visibility.Collapsed;
                btnUpdateCharacteristic.Visibility = Visibility.Collapsed;
                btnDeleteCharacteristic.Visibility = Visibility.Collapsed;
                btnAddCharValue.Visibility = Visibility.Collapsed;
                btnDeleteCharValue.Visibility = Visibility.Collapsed;
                txtNewCharValue.IsReadOnly = true;
                btnBackup.Visibility = Visibility.Collapsed;
                btnRestore.Visibility = Visibility.Collapsed;
                btnImportCsv.Visibility = Visibility.Collapsed;

            }
            else if (role == "manager")
            {
                tabUsers.Visibility = Visibility.Collapsed;
                btnBackup.Visibility = Visibility.Collapsed;
                btnRestore.Visibility = Visibility.Collapsed;
            }

            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadProducts();
            LoadCategories();
            LoadManufacturers();
            LoadCharacteristics();
            LoadUsers();
            LoadStatistics();
        }

        // ========================================================================
        // ПОЛЬЗОВАТЕЛИ
        // ========================================================================
        private void LoadUsers()
        {
            var users = db.Users.Select(u => new
            {
                u.Id,
                u.Login,
                u.FullName,
                u.Email,
                u.Role,
                Status = u.IsBlocked ? "Заблокирован" : "Активен"
            }).ToList();
            dgUsers.ItemsSource = users;
        }

        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem == null) return;
            dynamic selected = dgUsers.SelectedItem;
            int id = selected.Id;
            var user = db.Users.Find(id);
            if (user != null)
            {
                txtUserId.Text = user.Id.ToString();
                txtUserLogin.Text = user.Login;
                txtUserFullName.Text = user.FullName;
                txtUserEmail.Text = user.Email;
                cmbUserRole.Text = user.Role;
                chkUserBlocked.IsChecked = user.IsBlocked;
            }
        }

        private void BtnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void BtnUpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text)) return;
            int id = int.Parse(txtUserId.Text);
            var user = db.Users.Find(id);
            if (user == null) return;

            user.FullName = txtUserFullName.Text.Trim();
            user.Email = txtUserEmail.Text.Trim();
            user.Role = cmbUserRole.Text;
            user.IsBlocked = chkUserBlocked.IsChecked ?? false;

            db.SaveChanges();
            LoadUsers();
            MessageBox.Show("Пользователь обновлен");
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text)) return;
            int id = int.Parse(txtUserId.Text);
            var user = db.Users.Find(id);
            if (user == null) return;

            if (MessageBox.Show($"Удалить пользователя {user.Login}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.Users.Remove(user);
                db.SaveChanges();
                LoadUsers();
                ClearUserFields();
                MessageBox.Show("Пользователь удален");
            }
        }

        private void ClearUserFields()
        {
            txtUserId.Text = "";
            txtUserLogin.Text = "";
            txtUserFullName.Text = "";
            txtUserEmail.Text = "";
            cmbUserRole.SelectedIndex = -1;
            chkUserBlocked.IsChecked = false;
        }

        // ========================================================================
        // СТАТИСТИКА
        // ========================================================================
        private void LoadStatistics()
        {
            var byCategory = db.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count,
                    AvgPrice = c.Products.Any() ? c.Products.Average(p => p.Price) : 0
                })
                .OrderByDescending(x => x.ProductCount)
                .ToList();

            dgStatsByCategory.ItemsSource = byCategory;

            var byManufacturer = db.Manufacturers
                .Select(m => new
                {
                    ManufacturerName = m.Name,
                    Country = m.Country ?? "",
                    ProductCount = m.Products.Count,
                    AvgPrice = m.Products.Any() ? m.Products.Average(p => p.Price) : 0
                })
                .OrderByDescending(x => x.ProductCount)
                .ToList();

            dgStatsByManufacturer.ItemsSource = byManufacturer;
        }

        // ========================================================================
        // КАТЕГОРИИ
        // ========================================================================
        private void LoadCategories()
        {
            db.Categories.Load();
            lstCategories.ItemsSource = db.Categories.Local.ToObservableCollection();
            lstCategories.DisplayMemberPath = "Name";
        }

        private void LstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCategories.SelectedItem is Category selected)
            {
                txtCategoryName.Text = selected.Name;
                txtCategoryDescription.Text = selected.Description;
            }
        }

        private void BtnRefreshCategories_Click(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            ClearCategoryFields();
        }

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Введите название категории");
                return;
            }
            if (db.Categories.Any(c => c.Name == txtCategoryName.Text.Trim()))
            {
                MessageBox.Show("Категория с таким названием уже существует");
                return;
            }

            var cat = new Category
            {
                Name = txtCategoryName.Text.Trim(),
                Description = txtCategoryDescription.Text?.Trim()
            };
            db.Categories.Add(cat);
            db.SaveChanges();
            LoadCategories();
            ClearCategoryFields();
            MessageBox.Show("Категория добавлена");
        }

        private void BtnUpdateCategory_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategories.SelectedItem is Category selected)
            {
                if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
                {
                    MessageBox.Show("Введите название категории");
                    return;
                }
                if (db.Categories.Any(c => c.Name == txtCategoryName.Text.Trim() && c.Id != selected.Id))
                {
                    MessageBox.Show("Категория с таким названием уже существует");
                    return;
                }
                selected.Name = txtCategoryName.Text.Trim();
                selected.Description = txtCategoryDescription.Text?.Trim();
                db.SaveChanges();
                db.ChangeTracker.Clear();
                lstCategories.ItemsSource = db.Categories.ToList();
                lstCategories.DisplayMemberPath = "Name";
                MessageBox.Show("Категория обновлена");
            }
        }

        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategories.SelectedItem is Category selected)
            {
                if (db.Products.Any(p => p.CategoryId == selected.Id))
                {
                    MessageBox.Show("Невозможно удалить: есть товары в этой категории");
                    return;
                }
                if (MessageBox.Show($"Удалить категорию {selected.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Categories.Remove(selected);
                    db.SaveChanges();
                    LoadCategories();
                    ClearCategoryFields();
                    MessageBox.Show("Категория удалена");
                }
            }
        }

        private void ClearCategoryFields()
        {
            txtCategoryName.Text = "";
            txtCategoryDescription.Text = "";
        }

        // ========================================================================
        // ПРОИЗВОДИТЕЛИ
        // ========================================================================
        private void LoadManufacturers()
        {
            db.Manufacturers.Load();
            lstManufacturers.ItemsSource = db.Manufacturers.Local.ToObservableCollection();
            lstManufacturers.DisplayMemberPath = "Name";
        }

        private void LstManufacturers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstManufacturers.SelectedItem is Manufacturer selected)
            {
                txtManufacturerName.Text = selected.Name;
                txtManufacturerCountry.Text = selected.Country;
                txtManufacturerWebsite.Text = selected.Website;
            }
        }

        private void BtnRefreshManufacturers_Click(object sender, RoutedEventArgs e)
        {
            LoadManufacturers();
            ClearManufacturerFields();
        }

        private void BtnAddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtManufacturerName.Text))
            {
                MessageBox.Show("Введите название производителя");
                return;
            }
            if (db.Manufacturers.Any(m => m.Name == txtManufacturerName.Text.Trim()))
            {
                MessageBox.Show("Производитель с таким названием уже существует");
                return;
            }
            var man = new Manufacturer
            {
                Name = txtManufacturerName.Text.Trim(),
                Country = txtManufacturerCountry.Text?.Trim(),
                Website = txtManufacturerWebsite.Text?.Trim()
            };
            db.Manufacturers.Add(man);
            db.SaveChanges();
            LoadManufacturers();
            ClearManufacturerFields();
            MessageBox.Show("Производитель добавлен");
        }

        private void BtnUpdateManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (lstManufacturers.SelectedItem is Manufacturer selected)
            {
                if (string.IsNullOrWhiteSpace(txtManufacturerName.Text))
                {
                    MessageBox.Show("Введите название производителя");
                    return;
                }
                if (db.Manufacturers.Any(m => m.Name == txtManufacturerName.Text.Trim() && m.Id != selected.Id))
                {
                    MessageBox.Show("Производитель с таким названием уже существует");
                    return;
                }
                selected.Name = txtManufacturerName.Text.Trim();
                selected.Country = txtManufacturerCountry.Text?.Trim();
                selected.Website = txtManufacturerWebsite.Text?.Trim();
                db.SaveChanges();
                db.ChangeTracker.Clear();
                lstManufacturers.ItemsSource = db.Manufacturers.ToList();
                lstManufacturers.DisplayMemberPath = "Name";
                MessageBox.Show("Производитель обновлен");
            }
        }

        private void BtnDeleteManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (lstManufacturers.SelectedItem is Manufacturer selected)
            {
                if (db.Products.Any(p => p.ManufacturerId == selected.Id))
                {
                    MessageBox.Show("Невозможно удалить: есть товары этого производителя");
                    return;
                }
                if (MessageBox.Show($"Удалить производителя {selected.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Manufacturers.Remove(selected);
                    db.SaveChanges();
                    LoadManufacturers();
                    ClearManufacturerFields();
                    MessageBox.Show("Производитель удален");
                }
            }
        }

        private void ClearManufacturerFields()
        {
            txtManufacturerName.Text = "";
            txtManufacturerCountry.Text = "";
            txtManufacturerWebsite.Text = "";
        }

        // ========================================================================
        // ХАРАКТЕРИСТИКИ (с отображением стандартных значений)
        // ========================================================================
        private void LoadCharacteristics()
        {
            db.Characteristics.Load();
            lstCharacteristics.ItemsSource = db.Characteristics.Local.ToObservableCollection();
            lstCharacteristics.DisplayMemberPath = "Name";
        }

        private void LoadCharacteristicValues(int charId)
        {
            var values = db.CharacteristicValues
                .Where(cv => cv.CharacteristicId == charId)
                .Select(cv => new
                {
                    cv.Id,
                    cv.Value,
                    ProductCount = db.ProductCharacteristics.Count(pc => pc.CharacteristicValueId == cv.Id)
                })
                .ToList();
            dgCharacteristicValues.ItemsSource = values;
        }

        private void LstCharacteristics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCharacteristics.SelectedItem is Characteristic selected)
            {
                txtCharacteristicName.Text = selected.Name;
                txtCharacteristicUnit.Text = selected.Unit;
                LoadCharacteristicValues(selected.Id);
            }
            else
            {
                dgCharacteristicValues.ItemsSource = null;
            }
        }

        private void BtnRefreshCharacteristics_Click(object sender, RoutedEventArgs e)
        {
            LoadCharacteristics();
            ClearCharacteristicFields();
            dgCharacteristicValues.ItemsSource = null;
        }

        private void BtnAddCharacteristic_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCharacteristicName.Text))
            {
                MessageBox.Show("Введите название характеристики");
                return;
            }
            if (db.Characteristics.Any(c => c.Name == txtCharacteristicName.Text.Trim()))
            {
                MessageBox.Show("Характеристика с таким названием уже существует");
                return;
            }
            var ch = new Characteristic
            {
                Name = txtCharacteristicName.Text.Trim(),
                Unit = txtCharacteristicUnit.Text?.Trim()
            };
            db.Characteristics.Add(ch);
            db.SaveChanges();
            LoadCharacteristics();
            ClearCharacteristicFields();
            MessageBox.Show("Характеристика добавлена");
        }

        private void BtnUpdateCharacteristic_Click(object sender, RoutedEventArgs e)
        {
            if (lstCharacteristics.SelectedItem is Characteristic selected)
            {
                if (string.IsNullOrWhiteSpace(txtCharacteristicName.Text))
                {
                    MessageBox.Show("Введите название характеристики");
                    return;
                }
                if (db.Characteristics.Any(c => c.Name == txtCharacteristicName.Text.Trim() && c.Id != selected.Id))
                {
                    MessageBox.Show("Характеристика с таким названием уже существует");
                    return;
                }
                selected.Name = txtCharacteristicName.Text.Trim();
                selected.Unit = txtCharacteristicUnit.Text?.Trim();
                db.SaveChanges();
                db.ChangeTracker.Clear();
                lstCharacteristics.ItemsSource = db.Characteristics.ToList();
                lstCharacteristics.DisplayMemberPath = "Name";
                MessageBox.Show("Характеристика обновлена");
            }
        }

        private void BtnDeleteCharacteristic_Click(object sender, RoutedEventArgs e)
        {
            if (lstCharacteristics.SelectedItem is Characteristic selected)
            {
                if (db.ProductCharacteristics.Any(pc => pc.CharacteristicId == selected.Id))
                {
                    MessageBox.Show("Невозможно удалить: характеристика используется в товарах");
                    return;
                }
                if (MessageBox.Show($"Удалить характеристику {selected.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Characteristics.Remove(selected);
                    db.SaveChanges();
                    LoadCharacteristics();
                    ClearCharacteristicFields();
                    dgCharacteristicValues.ItemsSource = null;
                    MessageBox.Show("Характеристика удалена");
                }
            }
        }

        private void ClearCharacteristicFields()
        {
            txtCharacteristicName.Text = "";
            txtCharacteristicUnit.Text = "";
        }

        private void BtnAddCharValue_Click(object sender, RoutedEventArgs e)
        {
            if (lstCharacteristics.SelectedItem is not Characteristic selected) return;
            if (string.IsNullOrWhiteSpace(txtNewCharValue.Text))
            {
                MessageBox.Show("Введите значение");
                return;
            }
            if (db.CharacteristicValues.Any(cv => cv.CharacteristicId == selected.Id && cv.Value == txtNewCharValue.Text.Trim()))
            {
                MessageBox.Show("Такое значение уже существует");
                return;
            }
            db.CharacteristicValues.Add(new CharacteristicValue
            {
                CharacteristicId = selected.Id,
                Value = txtNewCharValue.Text.Trim()
            });
            db.SaveChanges();
            LoadCharacteristicValues(selected.Id);
            txtNewCharValue.Text = "";
        }

        private void BtnDeleteCharValue_Click(object sender, RoutedEventArgs e)
        {
            if (dgCharacteristicValues.SelectedItem == null) return;
            dynamic selected = dgCharacteristicValues.SelectedItem;
            int id = selected.Id;
            var cv = db.CharacteristicValues.Find(id);
            if (cv == null) return;
            if (db.ProductCharacteristics.Any(pc => pc.CharacteristicValueId == id))
            {
                MessageBox.Show("Значение используется в товарах, удалите сначала их");
                return;
            }
            db.CharacteristicValues.Remove(cv);
            db.SaveChanges();
            if (lstCharacteristics.SelectedItem is Characteristic ch)
                LoadCharacteristicValues(ch.Id);
        }

        private void DgCharacteristicValues_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (dgCharacteristicValues.SelectedItem == null) return;
            dynamic row = dgCharacteristicValues.SelectedItem;
            int id = row.Id;
            var cv = db.CharacteristicValues.Find(id);
            if (cv == null) return;

            var editedValue = (e.EditingElement as TextBox)?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(editedValue)) return;
            if (editedValue == cv.Value) return;

            if (db.CharacteristicValues.Any(x => x.CharacteristicId == cv.CharacteristicId && x.Value == editedValue && x.Id != id))
            {
                MessageBox.Show("Такое значение уже существует");
                LoadCharacteristicValues(cv.CharacteristicId);
                return;
            }

            cv.Value = editedValue;
            db.SaveChanges();
            if (lstCharacteristics.SelectedItem is Characteristic ch)
                LoadCharacteristicValues(ch.Id);
        }

        private void DgCharacteristicValues_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && dgCharacteristicValues.SelectedItem != null)
            {
                dynamic row = dgCharacteristicValues.SelectedItem;
                int id = row.Id;
                var cv = db.CharacteristicValues.Find(id);
                if (cv == null) return;
                if (dgCharacteristicValues.CurrentCell.Column.DisplayIndex == 0)
                {
                    // Trigger edit on Enter
                    dgCharacteristicValues.CommitEdit(DataGridEditingUnit.Row, true);
                }
            }
        }

        // ========================================================================
        // ТОВАРЫ - Поиск и отображение
        // ========================================================================
        public class SearchCondition
        {
            public string Field { get; set; } = "Наименование";
            public string Operator { get; set; } = "содержит";
            public string Value { get; set; } = "";
        }

        public class ProductDisplay
        {
            public int Id { get; set; }
            public string Article { get; set; } = "";
            public string Name { get; set; } = "";
            public string CategoryName { get; set; } = "";
            public string ManufacturerName { get; set; } = "";
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsSelected { get; set; }
        }

        private void LoadProducts()
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .ToList();

            currentProductSource = products.Select(p => new ProductDisplay
            {
                Id = p.Id,
                Article = p.Article,
                Name = p.Name,
                CategoryName = p.Category?.Name ?? "",
                ManufacturerName = p.Manufacturer?.Name ?? "",
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();

            dgProducts.ItemsSource = currentProductSource;
        }

        private void BtnAddCondition_Click(object sender, RoutedEventArgs e)
        {
            var condition = new SearchCondition();
            searchConditions.Add(condition);
            RenderSearchConditions();
        }

        private void RenderSearchConditions()
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };

            var fields = new List<string> {
                "Наименование", "Артикул", "Категория", "Производитель",
                "Цена", "Остаток", "Описание"
            };

            var chars = db.Characteristics.Select(c => c.Name).ToList();
            fields.AddRange(chars);

            var operators = new List<string> { "=", "≠", ">", "<", ">=", "<=", "содержит", "начинается с", "заканчивается на" };

            foreach (var cond in searchConditions)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

                var cmbField = new ComboBox { Width = 180, ItemsSource = fields, SelectedItem = cond.Field, Margin = new Thickness(0, 0, 5, 0) };
                cmbField.SelectionChanged += (s, args) => cond.Field = cmbField.SelectedItem?.ToString() ?? "";
                cmbField.Tag = cond;

                var cmbOp = new ComboBox { Width = 130, ItemsSource = operators, SelectedItem = cond.Operator, Margin = new Thickness(0, 0, 5, 0) };
                cmbOp.SelectionChanged += (s, args) => cond.Operator = cmbOp.SelectedItem?.ToString() ?? "";

                var txtVal = new TextBox { Width = 150, Text = cond.Value, Margin = new Thickness(0, 0, 5, 0) };
                txtVal.TextChanged += (s, args) => cond.Value = txtVal.Text;

                var btnRemove = new Button { Content = "✕", Width = 30, Height = 25, Background = Brushes.Red, Foreground = Brushes.White };
                var captured = cond;
                btnRemove.Click += (s, args) =>
                {
                    searchConditions.Remove(captured);
                    RenderSearchConditions();
                };

                row.Children.Add(cmbField);
                row.Children.Add(cmbOp);
                row.Children.Add(txtVal);
                row.Children.Add(btnRemove);
                stack.Children.Add(row);
            }

            icSearchConditions.ItemsSource = null;
            icSearchConditions.ItemsSource = new List<StackPanel> { stack };
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var query = db.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .AsQueryable();

            foreach (var cond in searchConditions)
            {
                if (string.IsNullOrWhiteSpace(cond.Value)) continue;

                switch (cond.Field)
                {
                    case "Наименование":
                        query = ApplyStringFilter(query, p => p.Name, cond.Operator, cond.Value.Trim());
                        break;
                    case "Артикул":
                        query = ApplyStringFilter(query, p => p.Article, cond.Operator, cond.Value.Trim());
                        break;
                    case "Категория":
                        query = ApplyStringFilter(query, p => p.Category.Name, cond.Operator, cond.Value.Trim());
                        break;
                    case "Производитель":
                        query = ApplyStringFilter(query, p => p.Manufacturer.Name, cond.Operator, cond.Value.Trim());
                        break;
                    case "Цена":
                        if (decimal.TryParse(cond.Value.Trim(), out decimal priceVal))
                            query = ApplyNumericFilter(query, p => (decimal)p.Price, cond.Operator, priceVal);
                        break;
                    case "Остаток":
                        if (int.TryParse(cond.Value.Trim(), out int stockVal))
                            query = ApplyNumericFilter(query, p => (decimal)p.StockQuantity, cond.Operator, stockVal);
                        break;
                    case "Описание":
                        query = ApplyStringFilter(query, p => p.Description ?? "", cond.Operator, cond.Value.Trim());
                        break;
                    default:
                        var charId = db.Characteristics.FirstOrDefault(c => c.Name == cond.Field)?.Id;
                        if (charId.HasValue)
                        {
                            int cid = charId.Value;
                            var matchingProductIds = db.ProductCharacteristics
                                .Include(pc => pc.CharacteristicValue)
                                .Where(pc => pc.CharacteristicId == cid)
                                .AsEnumerable()
                                .Where(pc => MatchString(pc.CharacteristicValue.Value, cond.Operator, cond.Value.Trim()))
                                .Select(pc => pc.ProductId)
                                .ToList();
                            query = query.Where(p => matchingProductIds.Contains(p.Id));
                        }
                        break;
                }
            }

            var results = query.ToList();
            currentProductSource = results.Select(p => new ProductDisplay
            {
                Id = p.Id,
                Article = p.Article,
                Name = p.Name,
                CategoryName = p.Category?.Name ?? "",
                ManufacturerName = p.Manufacturer?.Name ?? "",
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();

            dgProducts.ItemsSource = currentProductSource;
        }

        private static IQueryable<Product> ApplyStringFilter(IQueryable<Product> query, System.Linq.Expressions.Expression<Func<Product, string>> selector, string op, string value)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(Product), "p");
            var member = System.Linq.Expressions.Expression.Invoke(selector, param);
            var constant = System.Linq.Expressions.Expression.Constant(value);
            System.Linq.Expressions.Expression? body = null;

            switch (op)
            {
                case "=":
                    body = System.Linq.Expressions.Expression.Equal(member, constant);
                    break;
                case "≠":
                    body = System.Linq.Expressions.Expression.NotEqual(member, constant);
                    break;
                case "содержит":
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    body = System.Linq.Expressions.Expression.Call(member, containsMethod!, constant);
                    break;
                case "начинается с":
                    var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                    body = System.Linq.Expressions.Expression.Call(member, startsWithMethod!, constant);
                    break;
                case "заканчивается на":
                    var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                    body = System.Linq.Expressions.Expression.Call(member, endsWithMethod!, constant);
                    break;
                default:
                    return query;
            }

            if (body != null)
            {
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<Product, bool>>(body, param);
                return query.Where(lambda);
            }
            return query;
        }

        private static IQueryable<Product> ApplyNumericFilter(IQueryable<Product> query, System.Linq.Expressions.Expression<Func<Product, decimal>> selector, string op, decimal value)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(Product), "p");
            var member = System.Linq.Expressions.Expression.Invoke(selector, param);
            var constant = System.Linq.Expressions.Expression.Constant(value);
            System.Linq.Expressions.Expression? body = null;

            switch (op)
            {
                case "=": body = System.Linq.Expressions.Expression.Equal(member, constant); break;
                case "≠": body = System.Linq.Expressions.Expression.NotEqual(member, constant); break;
                case ">": body = System.Linq.Expressions.Expression.GreaterThan(member, constant); break;
                case "<": body = System.Linq.Expressions.Expression.LessThan(member, constant); break;
                case ">=": body = System.Linq.Expressions.Expression.GreaterThanOrEqual(member, constant); break;
                case "<=": body = System.Linq.Expressions.Expression.LessThanOrEqual(member, constant); break;
                default: return query;
            }

            if (body != null)
            {
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<Product, bool>>(body, param);
                return query.Where(lambda);
            }
            return query;
        }

        private static bool MatchString(string val, string op, string search)
        {
            switch (op)
            {
                case "=": return string.Equals(val, search, StringComparison.OrdinalIgnoreCase);
                case "≠": return !string.Equals(val, search, StringComparison.OrdinalIgnoreCase);
                case "содержит": return val.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                case "начинается с": return val.StartsWith(search, StringComparison.OrdinalIgnoreCase);
                case "заканчивается на": return val.EndsWith(search, StringComparison.OrdinalIgnoreCase);
                default: return true;
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            searchConditions.Clear();
            RenderSearchConditions();
            LoadProducts();
        }

        private void OnColumnVisibilityChanged(object sender, RoutedEventArgs e)
        {
            if (colArticle == null) return;
            colArticle.Visibility = chkColArticle.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colName.Visibility = chkColName.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colCategory.Visibility = chkColCategory.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colManufacturer.Visibility = chkColManufacturer.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colPrice.Visibility = chkColPrice.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colStock.Visibility = chkColStock.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colDescription.Visibility = chkColDescription.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            colCreatedAt.Visibility = chkColCreatedAt.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductEditWindow(db);
            if (dialog.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selected)
            {
                var product = db.Products
                    .Include(p => p.ProductCharacteristics)
                    .FirstOrDefault(p => p.Id == selected.Id);
                if (product != null)
                {
                    var dialog = new ProductEditWindow(db, product);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadProducts();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования");
            }
        }

        private void BtnViewProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selected)
            {
                var win = new ProductViewWindow(selected.Id);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите товар для просмотра");
            }
        }

        private void DgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selected)
            {
                var win = new ProductViewWindow(selected.Id);
                win.ShowDialog();
            }
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selected)
            {
                if (MessageBox.Show($"Удалить товар {selected.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var product = db.Products.Find(selected.Id);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Товар удален");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления");
            }
        }

        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = new List<int>();

            var items = currentProductSource;
            if (items == null)
            {
                var view = CollectionViewSource.GetDefaultView(dgProducts.ItemsSource);
                if (view != null)
                    items = view.Cast<ProductDisplay>().ToList();
            }

            if (items == null) { MessageBox.Show("Нет товаров для сравнения"); return; }

            foreach (var item in items)
            {
                if (item.IsSelected)
                    selectedIds.Add(item.Id);
            }

            if (selectedIds.Count < 2)
            {
                if (dgProducts.SelectedItem is ProductDisplay sel)
                    selectedIds.Add(sel.Id);
            }

            if (selectedIds.Count < 2)
            {
                if (selectedIds.Count == 1)
                    MessageBox.Show("Выберите минимум 2 товара для сравнения");
                else
                    MessageBox.Show("Выберите минимум 2 товара для сравнения (используйте чекбоксы)");
                return;
            }

            if (selectedIds.Count > 10)
            {
                MessageBox.Show("Максимум 10 товаров для сравнения");
                return;
            }

            var compareWindow = new CompareWindow(selectedIds);
            compareWindow.ShowDialog();
        }

        private void BtnToggleSelect_Click(object sender, RoutedEventArgs e)
        {
            var items = currentProductSource;
            if (items == null) return;
            bool allSelected = items.All(i => i.IsSelected);
            foreach (var item in items)
                item.IsSelected = !allSelected;
            dgProducts.ItemsSource = null;
            dgProducts.ItemsSource = items;
        }

        // ========================================================================
        // ЭКСПОРТ / ИМПОРТ CSV
        // ========================================================================
        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"products_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var products = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .Include(p => p.ProductCharacteristics)
                        .ThenInclude(pc => pc.Characteristic)
                    .Include(p => p.ProductCharacteristics)
                        .ThenInclude(pc => pc.CharacteristicValue)
                    .ToList();

                using var writer = new StreamWriter(dialog.FileName, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Артикул;Наименование;Категория;Производитель;Цена;Остаток;Описание;Характеристики");

                foreach (var p in products)
                {
                    var chars = p.ProductCharacteristics
                        .Select(pc => $"{pc.Characteristic?.Name ?? ""}={pc.CharacteristicValue?.Value ?? ""}");
                    string charsStr = string.Join("; ", chars);
                    string line = $"{EscapeCsv(p.Article)};{EscapeCsv(p.Name)};{EscapeCsv(p.Category?.Name ?? "")};{EscapeCsv(p.Manufacturer?.Name ?? "")};{p.Price:F2};{p.StockQuantity};{EscapeCsv(p.Description ?? "")};{EscapeCsv(charsStr)}";
                    writer.WriteLine(line);
                }

                MessageBox.Show($"Экспортировано {products.Count} товаров в {dialog.FileName}", "Экспорт CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var lines = File.ReadAllLines(dialog.FileName, System.Text.Encoding.UTF8);
                if (lines.Length < 2)
                {
                    MessageBox.Show("CSV файл пуст или содержит только заголовок");
                    return;
                }

                int imported = 0;
                int skipped = 0;

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var parts = ParseCsvLine(lines[i]);
                    if (parts.Count < 6) { skipped++; continue; }

                    string article = parts[0].Trim();
                    string name = parts[1].Trim();
                    string categoryName = parts[2].Trim();
                    string manufacturerName = parts[3].Trim();

                    if (!decimal.TryParse(parts[4].Trim(), out decimal price) || price <= 0) { skipped++; continue; }
                    if (!int.TryParse(parts[5].Trim(), out int stock) || stock < 0) { skipped++; continue; }

                    if (db.Products.Any(p => p.Article == article)) { skipped++; continue; }
                    if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                    var category = db.Categories.FirstOrDefault(c => c.Name == categoryName);
                    if (category == null)
                    {
                        category = new Category { Name = categoryName };
                        db.Categories.Add(category);
                        db.SaveChanges();
                    }

                    var manufacturer = db.Manufacturers.FirstOrDefault(m => m.Name == manufacturerName);
                    if (manufacturer == null)
                    {
                        manufacturer = new Manufacturer { Name = manufacturerName };
                        db.Manufacturers.Add(manufacturer);
                        db.SaveChanges();
                    }

                    var product = new Product
                    {
                        Article = article,
                        Name = name,
                        CategoryId = category.Id,
                        ManufacturerId = manufacturer.Id,
                        Price = price,
                        StockQuantity = stock,
                        Description = parts.Count > 6 ? parts[6].Trim() : null,
                        CreatedAt = DateTime.Now
                    };
                    db.Products.Add(product);
                    db.SaveChanges();

                    // Import characteristics if present
                    if (parts.Count > 7 && !string.IsNullOrWhiteSpace(parts[7]))
                    {
                        var charPairs = parts[7].Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var pair in charPairs)
                        {
                            var kv = pair.Split('=');
                            if (kv.Length == 2)
                            {
                                string chName = kv[0].Trim();
                                string chValue = kv[1].Trim();
                                if (!string.IsNullOrWhiteSpace(chName) && !string.IsNullOrWhiteSpace(chValue))
                                {
                                    var ch = db.Characteristics.FirstOrDefault(c => c.Name == chName);
                                    if (ch == null)
                                    {
                                        ch = new Characteristic { Name = chName };
                                        db.Characteristics.Add(ch);
                                        db.SaveChanges();
                                    }

                                    var cv = db.CharacteristicValues.FirstOrDefault(v => v.CharacteristicId == ch.Id && v.Value == chValue);
                                    if (cv == null)
                                    {
                                        cv = new CharacteristicValue { CharacteristicId = ch.Id, Value = chValue };
                                        db.CharacteristicValues.Add(cv);
                                        db.SaveChanges();
                                    }

                                    db.ProductCharacteristics.Add(new ProductCharacteristic
                                    {
                                        ProductId = product.Id,
                                        CharacteristicId = ch.Id,
                                        CharacteristicValueId = cv.Id
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }

                    imported++;
                }

                MessageBox.Show($"Импортировано: {imported}\nПропущено: {skipped}", "Импорт CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result;
        }

        // ========================================================================
        // РЕЗЕРВНОЕ КОПИРОВАНИЕ
        // ========================================================================
        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                FileName = $"LightingDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string backupPath = dialog.FileName.Replace("'", "''");
#pragma warning disable EF1002
                db.Database.ExecuteSqlRaw(@"BACKUP DATABASE [LightingDB] TO DISK = N'" + backupPath + @"' WITH INIT, STATS = 10");
#pragma warning restore EF1002
                MessageBox.Show($"Резервная копия сохранена:\n{backupPath}", "Бэкап БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка резервного копирования:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            var cs = db.Database.GetConnectionString()!;
            var masterCs = cs.Replace("Database=LightingDB", "Database=master");

            try
            {
                string restorePath = dialog.FileName.Replace("'", "''");

                if (MessageBox.Show("Восстановление базы данных приведет к потере всех текущих данных.\nПродолжить?", "Восстановление БД",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                db.Database.CloseConnection();
                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();

                using var masterConn = new Microsoft.Data.SqlClient.SqlConnection(masterCs);
                masterConn.Open();
                using var cmd = masterConn.CreateCommand();
                cmd.CommandText = @"
                    ALTER DATABASE [LightingDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [LightingDB] FROM DISK = N'" + restorePath + @"' WITH REPLACE, STATS = 10;
                    ALTER DATABASE [LightingDB] SET MULTI_USER;";
                cmd.ExecuteNonQuery();

                MessageBox.Show("База данных восстановлена.\nПриложение будет перезапущено.", "Восстановление БД",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Restart app
                var login = new LoginWindow();
                login.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                // Try to set back to multi-user
                try { db.Database.CloseConnection(); Microsoft.Data.SqlClient.SqlConnection.ClearAllPools(); using var mc = new Microsoft.Data.SqlClient.SqlConnection(masterCs); mc.Open(); using var mc2 = mc.CreateCommand(); mc2.CommandText = @"ALTER DATABASE [LightingDB] SET MULTI_USER"; mc2.ExecuteNonQuery(); }
                catch { }
                MessageBox.Show($"Ошибка восстановления:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========================================================================
        // ВЫХОД
        // ========================================================================
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
