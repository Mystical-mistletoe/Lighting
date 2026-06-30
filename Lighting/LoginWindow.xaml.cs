using Lighting.Data;
using Lighting.Models;
using System.Text.RegularExpressions;
using System.Windows;

namespace Lighting
{
    public partial class LoginWindow : Window
    {
        private AppDbContext db = new AppDbContext();

        public LoginWindow()
        {
            InitializeComponent();
            db.Database.EnsureCreated();
            SeedAccounts();
        }

        private void SeedAccounts()
        {
            if (!db.Accounts.Any())
            {
                db.Accounts.Add(new Account { Login = "admin", Password = "555", Role = "admin" });
                db.Accounts.Add(new Account { Login = "manager", Password = "455", Role = "manager" });
                db.SaveChanges();
            }
        }

        private string Sanitize(string input)
        {
            if (input == null) return "";
            // Удаление потенциально опасных символов, защита от SQL-инъекций и XSS
            return Regex.Replace(input.Trim(), @"[';\\\-]", "");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = Sanitize(txtLogin.Text);
            string password = txtPassword.Password; // пароль не санитируем (может содержать любые символы)

            if (string.IsNullOrWhiteSpace(login))
            {
                txtError.Text = "Введите логин";
                return;
            }

            // EF Core использует параметризованные запросы — SQL-инъекции невозможны
            var user = db.Accounts.FirstOrDefault(a =>
                a.Login == login && a.Password == password);

            if (user != null)
            {
                CurrentAccount.CurAccount = user;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                var appUser = db.Users.FirstOrDefault(u =>
                    u.Login == login && u.PasswordHash == password && !u.IsBlocked);

                if (appUser != null)
                {
                    CurrentAccount.CurAccount = new Account
                    {
                        Login = appUser.Login,
                        Password = appUser.PasswordHash,
                        Role = appUser.Role
                    };
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtError.Text = "Неверный логин/пароль или пользователь заблокирован";
                }
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }

    public static class CurrentAccount
    {
        public static Account CurAccount { get; set; } = null!;
        public static string CurrentUserDisplay { get; set; } = "";
    }
}
