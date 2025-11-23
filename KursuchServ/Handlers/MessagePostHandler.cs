using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class MessagePostHandler : CommandHandler
    {
        public override string Command => "POST/api/message";

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

                if (!payload.TryGetProperty("to", out var toProp) ||
                    !payload.TryGetProperty("theme", out var themeProp) ||
                    !payload.TryGetProperty("content", out var contentProp))
                {
                    await WriteError(http, "Некорректные данные", 400);
                    return;
                }

                string recipientId = toProp.GetString();
                string theme = themeProp.GetString();
                string content = contentProp.GetString();

                var recipient = context.Db.FindUserByID(recipientId);
                if (recipient == null)
                {
                    await WriteError(http, "Пользователь не найден", 404);
                    return;
                }

                recipient.Messages ??= new System.Collections.Generic.List<MessageEntry>();

                var user = context.Db.FindUserByID(session.UserId);

                recipient.Messages.Add(new MessageEntry
                {
                    Timestamp = DateTime.Now,
                    Author = user.Username,
                    Theme = theme,
                    Content = content
                });

                context.Db.UpdateUser(recipient);
                await WriteOk(http, new { to = recipientId });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при отправке сообщения: {ex}", 500);
            }
        }
    }
}
