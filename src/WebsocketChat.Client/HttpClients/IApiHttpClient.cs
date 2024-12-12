
namespace WebsocketChat.Client.HttpClients
{
    public interface IApiHttpClient
    {
        Task<HttpResponseMessage> GetAuthLogout();
        Task<HttpResponseMessage> GetAuthValidateToken(string token);
        Task<HttpResponseMessage> PostAuthChangePassword(StringContent content);
        Task<HttpResponseMessage> PostAuthLogin(StringContent content);
        Task<HttpResponseMessage> PostAuthRegister(StringContent content);
        Task<HttpResponseMessage> GetChatUsers();
        Task<HttpResponseMessage> GetChatMessages(string userId = null,
            int? pageNumber = Library.Constants.MinPageNumber,
            int? pageSize = Library.Constants.MinPageSize);
        Task<HttpResponseMessage> GetChatMessagesPagesCount(string userId = null,
                    int? pageSize = Library.Constants.MinPageSize);
    }
}