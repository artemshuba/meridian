namespace Meridian.ViewModel.Messages
{
    public enum LoginType
    {
        LogIn,
        LogOut
    }

    public class LoginMessage
    {
        public LoginType Type { get; set; }

        public string Service { get; set; }
    }
}
