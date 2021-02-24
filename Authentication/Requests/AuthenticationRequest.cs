using Planner.Network.Abstract;
using UnityEngine;
using UnityEngine.Networking;

namespace Planner.Authentication.Requests
{
    /*!
     \ingroup authentication
     */
    
    /// <summary>
    /// Запрос на авторизацию
    /// </summary>
    public class AuthenticationRequest: AbstractPostRequest<
        AuthenticationRequest.RequestData,
        AuthenticationRequest.ResponseData,
        AuthenticationRequest>
    {
        private Models.Token _token;
        
        public struct RequestData
        {
            public readonly Models.Token Token;

            public RequestData(Models.Token token)
            {
                Token = token;
            }
        }
        
        public struct ResponseData
        {
            public readonly Models.User User;

            public ResponseData(Models.User user)
            {
                User = user;
            }
        }
        
        protected override ResponseData CreateResponseData(byte[] responseBytes, string responseString)
        {
            var serializedUser = User.Parser.ParseFrom(responseBytes);
            var user = new Models.User(
                serializedUser.Name,
                serializedUser.Surname, 
                serializedUser.Rights, 
                _token,
                serializedUser.Email,
                serializedUser.Id);
            return new ResponseData(user);
        }

        protected override string GetPath(RequestData requestData)
        {
            return "auth";
        }

        protected override void PreparePostData(RequestData requestData, WWWForm postData)
        {
            // for return user data in response
            postData.AddField("need_user_data", 1);
        }

        protected sealed override UnityWebRequest CreateRequest(RequestData requestData)
        {
            _token = requestData.Token;
            var unityWebRequest = base.CreateRequest(requestData);
            unityWebRequest.SetRequestHeader("authorization", $"{requestData.Token.Identifier}");
            return unityWebRequest;
        }

        public class Pool: AbstractPool
        {
        }
    }
}