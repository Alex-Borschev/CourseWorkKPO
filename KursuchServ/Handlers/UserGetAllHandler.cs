using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class UserGetAllHandler : CommandHandler
    {
        public override string Command => "GET/api/user";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            try
            {
                var users = context.Db.GetAllUsers()
                    .Select(u => new
                    {
                        id = u.Id,
                        login = u.Username,
                        role = u.Personality
                    })
                    .ToList();

                await WriteOk(http, users);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error: {ex}", 500);
            }
        }
    }
}
