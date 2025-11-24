using System.Text.Json;
using Server.Database;
using Serilog;
using DotNetEnv;
using TokenServiceLibrary;

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
        public static async Task RunAsync(DatabaseService db = null)
        {
            var builder = WebApplication.CreateBuilder();

            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReact",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            var app = builder.Build();
            app.UseCors("AllowReact");

            var serverContext = new ServerContext(db ?? new DatabaseService());

            // Регистрируем обработчики
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
            serverContext.Router.RegisterHandler(new Handlers.UploadPostHandler());
            serverContext.Router.RegisterHandler(new Handlers.TermUpdateHandler());

            // Middleware для кастомного роутинга
            app.Use(async (context, next) =>
            {
                var path = $"{context.Request.Method}{context.Request.Path}";

                if (serverContext.Router.HasRoute(path))
                {
                    await HandleHttpRequest(context, serverContext);
                    return;
                }

                await next();
            });

            // Статика
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                    Path.Combine(AppContext.BaseDirectory, "uploads")
                ),
                RequestPath = "/uploads"
            });
            string port = Env.GetString("PORT");
            await app.RunAsync($"http://0.0.0.0:{port}");
        }

        private static async Task HandleHttpRequest(HttpContext http, ServerContext context)
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
                var response = new { message = $"Error: {ex.Message}" };
                http.Response.StatusCode = 500;
                http.Response.ContentType = "application/json";
                await http.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        private static async Task<JsonElement?> GetBodyAsync(HttpContext http)
        {
            if (!http.Request.ContentLength.HasValue || http.Request.ContentLength == 0)
                return JsonDocument.Parse("{}").RootElement;

            // Сохраняем тело запроса в MemoryStream, чтобы можно было читать несколько раз
            using var memoryStream = new MemoryStream();
            await http.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream);
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
