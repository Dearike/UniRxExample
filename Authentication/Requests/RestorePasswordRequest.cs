using Planner.Network.Abstract;
using UnityEngine;

namespace Planner.Authentication.Requests
{
    /*!
     \ingroup authentication
     */
    
    /// <summary>
    /// Запрос на авторизацию
    /// </summary>
    public class RestorePasswordRequest : AbstractPostRequest<
        RestorePasswordRequest.RequestData,
        bool,
        RestorePasswordRequest>
    {
        private Models.Token _token;
        
        public struct RequestData
        {
            public readonly string Email;

            public RequestData(string email)
            {
                Email = email;
            }
        }

        protected override bool CreateResponseData(byte[] responseBytes, string responseString)
        {
            return responseString == "1";
        }

        protected override string GetPath(RequestData requestData)
        {
            return "restore_password";
        }

        protected override void PreparePostData(RequestData requestData, WWWForm postData)
        {            
            postData.AddField("email", requestData.Email);
        }

        public class Pool: AbstractPool
        {
        }
    }
}