
namespace WebsocketChat.Client.HttpClients
{
    public interface IApiHttpClient
    {
        Task<HttpResponseMessage> GetUsersLogout();
        Task<HttpResponseMessage> GetUsersValidateToken(string token);
        Task<HttpResponseMessage> PostUsersChangePassword(StringContent content);
        Task<HttpResponseMessage> PostUsersLogin(StringContent content);
        Task<HttpResponseMessage> PostUsersRegister(StringContent content);
    }
}