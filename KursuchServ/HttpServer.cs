using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.Http;
using Server.Database;
using static System.Net.WebRequestMethods;
using MongoDB.Driver;
using Server.Auth;
using Serilog;

namespace Server
{
    public class ServerContext 
    { 
        public ServerRouter Router { get; set; } 
        public DatabaseService Db { get; set; } 
        public TokenSessionService SessionService { get; set; }
        public ServerContext(DatabaseService db)
        { 
            Db = db; 
            Router = new ServerRouter();
            SessionService = new TokenSessionService(TimeSpan.FromHours(5));

        }
    }
    public static class HttpServer
    {
        public static async Task RunAsync(Database.DatabaseService db = null)
        {
            var builder = WebApplication.CreateBuilder();
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();
            var app = builder.Build();

            var serverContext = new ServerContext(db ?? new Database.DatabaseService());
            

            serverContext.Router.RegisterHandler(new Handlers.UserAuthPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.UserRegPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermsGetAllHandler());
            serverContext.Router.RegisterHandler(new Handlers.CategoriesGetHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermsPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermsDeleteHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermsVisitedPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermsLikePostHandler());
            serverContext.Router.RegisterHandler(new Handlers.NotePostHandler());
            serverContext.Router.RegisterHandler(new Handlers.NoteDeleteHandler());
            serverContext.Router.RegisterHandler(new Handlers.MessageDeleteHandler());
            serverContext.Router.RegisterHandler(new Handlers.MessagePostHandler());
            serverContext.Router.RegisterHandler(new Handlers.UserPutHandler());
            serverContext.Router.RegisterHandler(new Handlers.MessageSuggestionPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.UserGetAllHandler());
            serverContext.Router.RegisterHandler(new Handlers.RatePostHandler());

            app.Use(async (context, next) =>
            {
                HandleHttpRequest(context, serverContext);
                await next();
            });

            await app.RunAsync("http://0.0.0.0:8888");
        }

        private static async void HandleHttpRequest(HttpContext http, ServerContext context)
        {
            try
            {
                var body = await GetBodyAsync(http);

                await context.Router.Route(
                    $"{http.Request.Method}{http.Request.Path}",
                    body.Value,
                    http,
                    context
                );
            }
            catch (Exception ex)
            {
                var response = new { message = $"Error: {ex}" };
                http.Response.StatusCode = 500;
                http.Response.ContentType = "application/json";
                await http.Response.WriteAsync(JsonSerializer.Serialize(response));
            }

        }

        private static async Task<JsonElement?> GetBodyAsync(HttpContext http)
        {
            using var reader = new StreamReader(http.Request.Body);
            string bodyString = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(bodyString))
                return JsonDocument.Parse("{}").RootElement;

            try
            {
                var body = JsonSerializer.Deserialize<JsonElement>(bodyString);
                return body;
            }
            catch (JsonException)
            {
                return JsonDocument.Parse("{}").RootElement;
            }
        }
    }
}
