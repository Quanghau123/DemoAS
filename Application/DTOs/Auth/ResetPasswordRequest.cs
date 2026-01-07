namespace DemoEF.Application.Auth
{
    public class ResetPasswordRequest
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}