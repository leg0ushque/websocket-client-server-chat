using WebsocketChat.Client.Helpers;

namespace WebsocketChat.Client.HttpClients
{
    public class ApiHttpClient : BaseApiHttpClient, IApiHttpClient
    {
        public ApiHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        // AUTH

        public async Task<HttpResponseMessage> PostUsersLogin(StringContent content)
        {
            var result = await Client.PostAsync(SetRequestPath("Auth/login"), content);
            return result;
        }

        public async Task<HttpResponseMessage> GetUsersValidateToken(string token)
        {
            using var content = JsonHelper.ObjectToStringContent(token);
            var result = await Client.PostAsync(SetRequestPath("Auth/validate"), content);
            return result;
        }

        public async Task<HttpResponseMessage> PostUsersRegister(StringContent content)
        {
            var result = await Client.PostAsync(SetRequestPath("Auth/register"), content);
            return result;
        }

        public async Task<HttpResponseMessage> PostUsersChangePassword(StringContent content)
        {
            RequestHelper.SetRequestToken(Client, HttpContextAccessor);
            var result = await Client.PostAsync(SetRequestPath("Auth/changePassword"), content);
            return result;
        }

        public async Task<HttpResponseMessage> GetUsersLogout()
        {
            var result = await Client.GetAsync(SetRequestPath("Auth/logout"));
            return result;
        }
    }
}
