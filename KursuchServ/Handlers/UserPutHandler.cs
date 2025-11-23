using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{

    public class UserPutHandler : CommandHandler
    {
        public override string Command => "PUT/api/user";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"].ToString();

                var session = context.SessionService.ValidateToken(token);
                if (session == null)
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "User not found", 404);
                    return;
                }

                await WriteOk(http, user);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error receiving data: {ex}", 500);
            }
        }
    }
}
