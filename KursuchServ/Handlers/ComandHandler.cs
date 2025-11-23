using Server;
using System.Text.Json;

public abstract class CommandHandler : ICommandHandler
{
    public abstract string Command { get; }

    public abstract Task Handle(JsonElement payload, HttpContext http, ServerContext context);

    protected async Task WriteError(HttpContext http, string message, int statusCode = 500)
    {
        var response = new
        {
            message = message
        };

        http.Response.StatusCode = statusCode;
        http.Response.ContentType = "application/json";
        await http.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    protected async Task WriteOk(HttpContext http, object data = null)
    {
        http.Response.ContentType = "application/json";
        await http.Response.WriteAsync(JsonSerializer.Serialize(data));
    }
}
