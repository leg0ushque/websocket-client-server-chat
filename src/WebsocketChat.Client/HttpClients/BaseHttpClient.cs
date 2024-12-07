namespace WebsocketChat.Client.HttpClients
{
    public class BaseApiHttpClient
    {
        protected BaseApiHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            Client = httpClient;
            HttpContextAccessor = httpContextAccessor;
        }

        protected HttpClient Client { get; set; }

        protected IHttpContextAccessor HttpContextAccessor { get; set; }

        protected internal Uri SetRequestPath(string path) =>
            new Uri($"{Client.BaseAddress}{path}");
    }
}
