using System.Text.Json;

namespace Server.Handlers
{
    public class UserGetAllHandler : CommandHandler
    {
        public override string Command => "GET/api/user";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            string token = http.Request.Headers["Authorization"].ToString();
            var session = context.SessionService.ValidateToken(token);
            if (session == null)
            {
                await WriteError(http, "The user is not authorized", 401);
                return;
            }
            
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
