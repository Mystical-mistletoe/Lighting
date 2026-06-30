namespace Lighting.Models
{
    public sealed class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "user";
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
