using DemoEF.Domain.Enums.User;

namespace DemoEF.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
        public string? ProviderUserId { get; set; }
        public bool IsActive { get; set; } = true;
        public UserRole UserRole { get; set; }
    }
}
