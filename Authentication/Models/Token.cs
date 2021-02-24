namespace Planner.Authentication.Models
{
    /*!
     \ingroup authentication
     */
    
    /// <summary>
    /// Уникальный токен, по которому пользователь осуществляет авторизацию
    /// После успешного входа пользователь получает с сервера
    /// </summary>
    public class Token
    {
        public readonly string Identifier;

        public Token(string identifier)
        {
            Identifier = identifier;
        }
    }
}