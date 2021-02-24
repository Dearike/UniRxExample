using Planner.Network.Abstract;
using UnityEngine;
using SerializedToken = Token;

namespace Planner.Authentication.Requests
{
    /*!
     \ingroup authentication
     */
    
    /// <summary>
    /// Запрос на вход
    /// </summary>
    public class LogInRequest: AbstractPostRequest<
        LogInRequest.RequestData,
        LogInRequest.ResponseData,
        LogInRequest>
    {
        public class Pool : AbstractPool
        {
        }

        public struct RequestData
        {
            public readonly string Login;
            public readonly string Password;

            public RequestData(string login, string password)
            {
                Login = login;
                Password = password;
            }
        }

        public struct ResponseData
        {
            public readonly Models.Token Token;

            public ResponseData(Models.Token token)
            {
                Token = token;
            }
        }
        
        protected override ResponseData CreateResponseData(byte[] responseBytes, string responseString)
        {
            var serializedToken = SerializedToken.Parser.ParseFrom(responseBytes);
            var token = new Models.Token(serializedToken.Indentifier);
            return new ResponseData(token);
        }

        protected override string GetPath(RequestData requestData)
        {
            return "login";
        }

        protected override void PreparePostData(RequestData requestData, WWWForm postData)
        {
            Debug.Log($"try login with {requestData.Login}:{requestData.Password}");
            postData.AddField("login", requestData.Login);
            postData.AddField("password", requestData.Password);
        }
    }
}