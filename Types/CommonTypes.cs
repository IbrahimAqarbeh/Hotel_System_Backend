namespace hotel_system_backend.Models;

public class CommonTypes
{
    public class SignInRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}