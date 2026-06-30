using Lighting.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace Lighting
{
    public sealed partial class RegisterWindow : Window
    {
        private AppDbContext db = new AppDbContext();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private string Sanitize(string input)
        {
            if (input == null) return "";
            return Regex.Replace(input.Trim(), @"[';\\\-]", "");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void BtnRegisterSubmit_Click(object sender, RoutedEventArgs e)
        {
            string login = Sanitize(txtRegLogin.Text);
            string password = txtRegPassword.Password;
            string passwordConfirm = txtRegPasswordConfirm.Password;
            string email = Sanitize(txtRegEmail.Text);
            string fullName = Sanitize(txtRegFullName.Text);

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Заполните все обязательные поля (отмечены *)");
                return;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен быть не менее 3 символов");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email адрес");
                return;
            }

            if (password != passwordConfirm)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            if (password.Length < 3)
            {
                MessageBox.Show("Пароль должен быть не менее 3 символов");
                return;
            }

            if (db.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            if (db.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Пользователь с таким email уже существует");
                return;
            }

            var user = new Models.User
            {
                Login = login,
                PasswordHash = password,
                Email = email,
                FullName = fullName,
                Role = "user",
                IsBlocked = false,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();
            MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему.");
            this.Close();
        }
    }
}
