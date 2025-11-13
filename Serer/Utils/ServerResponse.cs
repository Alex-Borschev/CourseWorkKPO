// Utils/ServerResponse.cs
// Унифицированная структура всех ответов сервера клиенту.

using System.Text.Json;

namespace Server
{
    public class ServerResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public object Payload { get; set; }

        public static ServerResponse Ok(string status = "OK", object payload = null)
            => new ServerResponse { Success = true, Status = status, Payload = payload };

        public static ServerResponse Error(string status = "Ошибка", object payload = null)
            => new ServerResponse { Success = false, Status = status, Payload = payload };

        /// <summary>
        /// Сериализует объект ServerResponse в JSON строку.
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
