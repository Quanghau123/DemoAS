namespace DemoEF.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!; // user.view
        public string Name { get; set; } = null!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
