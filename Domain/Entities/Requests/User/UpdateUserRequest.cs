using DemoEF.Domain.Enums.User;

namespace DemoEF.Domain.Entities.Requests.User
{
    public class UpdateUserRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public UserRole? UserRole { get; set; }
    }
}