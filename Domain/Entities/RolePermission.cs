using DemoEF.Domain.Enums.User;

namespace DemoEF.Domain.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }

        public UserRole Role { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
