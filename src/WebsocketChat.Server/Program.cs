
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketChat.Server.Contexts;
using WebsocketChat.Server.Handlers;
using WebsocketChat.Server.Identity;
using WebsocketChat.Server.Middlewares;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server
{
    public class Program
    {
        private const string ConnectionStringName = "ServerDbConnectionString";
        private static IConfiguration configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();

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

            app.UseMiddleware<Middlewares.WebSocketMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebsocketChat.Server.API");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            var jwtOptionsSection = configuration.GetSection(JwtTokenOptions.JwtConfigSectionKey);
            var jwtIssuer = jwtOptionsSection.GetValue<string>(JwtTokenOptions.JwtIssuerConfigKey);
            var jwtAudience = jwtOptionsSection.GetValue<string>(JwtTokenOptions.JwtAudienceConfigKey);
            var jwtSecretKey = jwtOptionsSection.GetValue<string>(JwtTokenOptions.JwtSecretKeyConfigKey);

            services.AddOptions<JwtTokenOptions>()
                .Configure<IConfiguration>((settings, config) =>
                {
                    jwtOptionsSection.Bind(settings);
                });

            // Databases & Identity

            var connString = configuration.GetConnectionString(ConnectionStringName);

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(connectionString: connString));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

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
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                        ValidateLifetime = false,
                    };
                });

            services.AddScoped<JwtTokenService>();

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
            services.AddSingleton<IMessageHandler, MessageHandler>();

            services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(120);
            });

            // Other

            services.AddControllers();
            ConfigureSwagger(services);
        }

        public static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebsocketChat.Server.API",
                    Version = "v1",
                    Description = "Provides endpoints to interact with non-chat server features.",
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Description = "Allows to attach a JWT token to the request to access the endpoints requiring authorization.",
                    In = ParameterLocation.Header,
                    Name = "JWT Authentication",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme,
                    },
                };
                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, System.Array.Empty<string>() },
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }
    }
}
