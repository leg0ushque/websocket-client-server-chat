# How-to:

1. Check your appsettings & apply migrations

Create appsettings.json in the Server project's root with following content:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtTokenSettings": {
    "JwtIssuer": "<custom issuer>",
    "JwtAudience": "<custom audience>",
    "JwtSecretKey": "<secret key of 32 symbols>"
  },
  "ConnectionStrings": {
    "ServerDbConnectionString": "Server=(localdb)\\mssqllocaldb;Database=WebsocketChatDb;Trusted_Connection=True;"
  }
}
```

Create appsettings.json in the Client project's root with following content:
```
{
  "ApiAddress": "http://localhost:5000/",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

In Visual Studio search find "package manager console" and run it. Select your Server project as the default.

Run the following command to populate your DB to your MSSQL Server local db

```
dotnet ef database update --project src\WebsocketChat.Server\WebsocketChat.Server.csproj
```


to create one (if needed) use:
dotnet ef migrations add InitialCreate --project src\WebsocketChat.Server\WebsocketChat.Server.csproj

2. Right click on the solution in the Solution Explorer, find "Configure Startup projects". Select "Start" *both* on Client & Server projects. Save it.

3. Press Start