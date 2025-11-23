using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{

    public class MessageDeleteHandler : CommandHandler
    {
        public override string Command => "DELETE/api/message";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"];
                var session = context.SessionService.ValidateToken(token);

                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "Пользователь не авторизован", 401);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "Пользователь не найден", 404);
                    return;
                }

                user.Messages?.Clear();
                context.Db.UpdateUser(user);
                await WriteOk(http);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при очистке сообщений: {ex}", 500);
            }
        }
    }

}
