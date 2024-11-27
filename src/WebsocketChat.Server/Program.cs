
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server
{
    public class Program
    {
        public IConfiguration Configuration { get; }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services);

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                app.Services.GetService<WebSocketConnectionManager>();
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path != "/ws")
                {
                    await next();
                }
                else
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400; // Bad Request
                        await context.Response.WriteAsync("Not a WebSocket request");
                        return;
                    }

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var manager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();

                    await HandleWebsocketRequestAsync(webSocket, manager);
                }
            });

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            /*
            var connString = Configuration.GetConnectionString(ConnectionStringName);

            services.AddDbContext<AuthApiDbContext>(options =>
                options.UseSqlServer(connectionString: connString));
            services.AddIdentity<Api.Identity.User, IdentityRole>()
                .AddEntityFrameworkStores<AuthApiDbContext>()
                .AddDefaultTokenProviders();


            var tokenSettings = Configuration.GetSection("JwtTokenSettings");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = tokenSettings[JwtIssuerConfigKey],
                        ValidateAudience = true,
                        ValidAudience = tokenSettings[JwtAudienceConfigKey],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings[JwtSecretKeyConfigKey])),
                        ValidateLifetime = false,
                    };
                });

            services.Configure<JwtTokenSettings>(tokenSettings);
            services.AddScoped<JwtTokenService>();
            */

            services.AddControllers();

            // CORS

            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAll",
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                             .AllowAnyHeader()
                                             .AllowAnyMethod();
                                  });
            });

            // Websocket services

            services.AddSingleton<WebSocketConnectionManager>();

            services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(120);
            });
        }

        private static async Task HandleWebsocketRequestAsync(WebSocket webSocket, WebSocketConnectionManager manager,
            CancellationToken ct = default)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            manager.AddSocket(webSocket);

            while (!result.CloseStatus.HasValue)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                foreach (var connectedClient in manager.GetAllClients())
                {
                    if (connectedClient.Value.State == WebSocketState.Open)
                    {
                        await connectedClient.Value.SendAsync(
                            new ArraySegment<byte>(buffer, 0, result.Count),
                            result.MessageType,
                            result.EndOfMessage,
                            ct);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            }

            string id = manager.GetId(webSocket);
            if (id != null)
            {
                await manager.RemoveSocketAsync(id, ct);
            }
        }
    }
}
