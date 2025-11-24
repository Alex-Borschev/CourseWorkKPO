using System.Text.Json;

namespace Server.Handlers
{

    public class MessageDeleteHandler : CommandHandler
    {
        public override string Command => "DELETE/api/message";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);

                if (string.IsNullOrEmpty(session.UserId))
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

                user.Messages?.Clear();
                context.Db.UpdateUser(user);
                await WriteOk(http, new { message = "Messages has been deleted" });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error clearing messages: {ex}", 500);
            }
        }
    }

}
