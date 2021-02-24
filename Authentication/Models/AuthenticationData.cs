namespace Planner.Authentication.Models
{
    public class AuthenticationData
    {
        private string _login;
        private string _password;

        public bool ForgotMe = false;

        public struct Memento
        {
            public string Login;
            public string Password;
            public bool ForgotMe;
        }

        public void SetLogIn(string login)
        {
            _login = login;
        }

        public void SetPassword(string password)
        {
            _password = password;
        }

        public Memento GetMemento()
        {
            return new Memento
            {
                Login = _login,
                Password = _password,
                ForgotMe = ForgotMe
            };
        }
    }
}