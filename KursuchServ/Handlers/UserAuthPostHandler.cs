using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class UserAuthPostHandler : CommandHandler
    {
        public override string Command => "POST/api/user/auth";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            // Check for required fields
            if (!payload.TryGetProperty("login", out var loginProp) ||
                !payload.TryGetProperty("password", out var passwordProp))
            {
                await WriteError(http, "Required fields are missing", 400);
                return;
            }

            string? login = loginProp.GetString() ?? "";
            string? password = passwordProp.GetString() ?? "";
            
            // Check user
            var user = context.Db.ValidateUser(login, password);
            if (user == null)
            {
                await WriteError(http, "Wrong data", 400);
                return;
            }

            string newToken = context.SessionService.CreateSession(user.Id);
            
            await WriteOk(http, new
            {
                id = user.Id,
                login = user.Username,
                role = user.Personality,
                messages = user.Messages,
                token = newToken
            });
        }
    }
}
