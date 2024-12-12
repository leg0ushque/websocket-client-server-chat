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

        public async Task<HttpResponseMessage> PostAuthLogin(StringContent content)
        {
            var result = await Client.PostAsync(SetRequestPath("Auth/login"), content);
            return result;
        }

        public async Task<HttpResponseMessage> GetAuthValidateToken(string token)
        {
            using var content = JsonHelper.ObjectToStringContent(token);
            var result = await Client.PostAsync(SetRequestPath("Auth/validate"), content);
            return result;
        }

        public async Task<HttpResponseMessage> PostAuthRegister(StringContent content)
        {
            var result = await Client.PostAsync(SetRequestPath("Auth/register"), content);
            return result;
        }

        public async Task<HttpResponseMessage> PostAuthChangePassword(StringContent content)
        {
            RequestHelper.SetRequestToken(Client, HttpContextAccessor);
            var result = await Client.PostAsync(SetRequestPath("Auth/changePassword"), content);
            return result;
        }

        public async Task<HttpResponseMessage> GetAuthLogout()
        {
            var result = await Client.GetAsync(SetRequestPath("Auth/logout"));
            return result;
        }

        // Messages

        public async Task<HttpResponseMessage> GetChatMessages(string userId = null,
            int? pageNumber = Library.Constants.MinPageNumber,
            int? pageSize = Library.Constants.MinPageSize)
        {
            var requestPath = string.IsNullOrEmpty(userId) ?
                "Messages/get" : $"Messages/get/{userId}";
            var queryParameters = $"?pageNumber={pageNumber}&pageSize={pageSize}";

            RequestHelper.SetRequestToken(Client, HttpContextAccessor);
            var result = await Client.GetAsync(SetRequestPath(requestPath + queryParameters));
            return result;
        }

        public async Task<HttpResponseMessage> GetChatMessagesPagesCount(string userId = null,
                    int? pageSize = Library.Constants.MinPageSize)
        {
            var requestPath = string.IsNullOrEmpty(userId) ?
                "Messages/getPages?" : $"Messages/getPages?userId={userId}&";
            var pageSizeParameter = $"pageSize={pageSize}";

            RequestHelper.SetRequestToken(Client, HttpContextAccessor);
            var result = await Client.GetAsync(SetRequestPath(requestPath + pageSizeParameter));
            return result;
        }

        public async Task<HttpResponseMessage> GetChatUsers()
        {
            var requestPath = "Messages/getUsers";

            RequestHelper.SetRequestToken(Client, HttpContextAccessor);
            var result = await Client.GetAsync(SetRequestPath(requestPath));
            return result;
        }
    }
}
