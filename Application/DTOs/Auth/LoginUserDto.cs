namespace DemoEF.Application.DTOs.Auth
{
    public class LoginUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool? IsActive { get; set; } = null;
    }
}