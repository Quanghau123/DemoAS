public interface IPermissionService
{
    Task<List<string>> GetPermissionsByUserAsync(int userId);
}
