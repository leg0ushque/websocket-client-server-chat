using Microsoft.Extensions.Configuration;
using NATS.Client.Internals;
using NBomber.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebsocketChat.Library.Entities;
using WebsocketChat.Library.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

        var apiSettings = configuration.GetSection("ApiSettings");
        var authUrl = apiSettings["AuthenticationUrl"];
        var userId = apiSettings["UserId"];
        var email = apiSettings["Email"];
        var password = apiSettings["Password"];

        var webSocketUrl = apiSettings["WebSocketUrl"];

        var httpClient = new HttpClient();

        var authData = new LoginModel { Email = email, Password = password };
        var content = new StringContent(JsonConvert.SerializeObject(authData), Encoding.UTF8, "application/json");
        var authResponse = await httpClient.PostAsync(authUrl, content);
        authResponse.EnsureSuccessStatusCode();
        var serializedAuthResponse = await authResponse.Content.ReadAsStringAsync();
        var authResult = JsonConvert.DeserializeObject<AuthResultModel>(serializedAuthResponse);

        var scenario = Scenario.Create("WebSocket Load Test", async context =>
        {
            using var client = new ClientWebSocket();
            client.Options.SetRequestHeader("Authorization", $"Bearer {authResult.JwtToken}");

            return await RunTestCaseAsync(client, webSocketUrl, authResult.WebSocketToken, userId);
        });

        var loadTest = NBomberRunner
            .RegisterScenarios(scenario
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(
                    Simulation.Inject(
                        rate: 20,
                        interval: TimeSpan.FromSeconds(1),
                        during: TimeSpan.FromSeconds(30)
                    )
                ))
            .Run();

        Console.ReadLine();
    }

    private static async Task<NBomber.Contracts.IResponse> RunTestCaseAsync(ClientWebSocket client, string webSocketUrl, string webSocketToken, string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await client.ConnectAsync(new Uri(webSocketUrl), cancellationToken);
            Console.WriteLine("Connected to WebSocket server.");

            var handShakeMessageBytes = CreateWebSocketMessage(true, webSocketToken, string.Empty, userId);
            await client.SendAsync(new ArraySegment<byte>(handShakeMessageBytes), WebSocketMessageType.Text, true, cancellationToken);
            Console.WriteLine("Handshake message sent.");

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            var testMessageBytes = CreateWebSocketMessage(false, webSocketToken, "Hello, WebSocket!", userId);  // Assuming a real message to send
            await client.SendAsync(new ArraySegment<byte>(testMessageBytes), WebSocketMessageType.Text, true, cancellationToken);
            Console.WriteLine("Test message sent.");

            var buffer = new byte[1024 * 4];

            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            var serializedResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var response = JsonConvert.DeserializeObject<SentMessageModel>(serializedResponse);

            Console.WriteLine($"Message received: isReceived={response?.IsReceived}, message='{response?.Message}'");

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", cancellationToken);
            Console.WriteLine("WebSocket closed.");

            if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
            {
                return Response.Ok();
            }
            else
            {
                return Response.Fail(message: "Invalid response type or empty response");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return Response.Fail(message: $"Exception: {ex.Message}");
        }
    }

    private static byte[] CreateWebSocketMessage(bool isSystemMessage, string webSocketToken, string messageText, string userId)
    {
        var message = new WebSocketMessage
        {
            IsSystemMessage = isSystemMessage,
            Token = webSocketToken,
            MessageText = messageText,
            UserId = userId,
        };

        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
    }
}