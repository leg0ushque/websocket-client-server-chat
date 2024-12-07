namespace WebsocketChat.Client.Filters
{
    public class RedirectOnUnauthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public RedirectOnUnauthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 401 && !context.Response.Headers.ContainsKey("Location"))
            {
                context.Response.Redirect($"/Auth/login");
            }
        }
    }
}
