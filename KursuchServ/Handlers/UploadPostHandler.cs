using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class UploadPostHandler : CommandHandler
    {
        public override string Command => "POST/api/upload";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                Console.WriteLine(12345);
                if (!payload.TryGetProperty("name", out var nameProp) ||
                    !payload.TryGetProperty("base64", out var baseProp))
                {
                    await WriteError(http, "Invalid request format", 400);
                    return;
                }

                string? fileNameFromClient = nameProp.GetString();
                string? base64String = baseProp.GetString();

                if (string.IsNullOrWhiteSpace(base64String))
                {
                    await WriteError(http, "Base64 is empty", 400);
                    return;
                }

                // Convert base64 -> byte[]
                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(base64String);
                }
                catch
                {
                    await WriteError(http, "Invalid base64 data", 400);
                    return;
                }

                // Генерируем имя файла
                string extension = Path.GetExtension(fileNameFromClient) ?? ".jpg";
                string newFileName = Guid.NewGuid().ToString() + extension;

                // Папка для загрузок
                string uploadFolder = Path.Combine(AppContext.BaseDirectory, "uploads");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // Путь до файла
                string filePath = Path.Combine(uploadFolder, newFileName);
                await File.WriteAllBytesAsync(filePath, bytes);

                // URL для клиента
                string fileUrl = "/uploads/" + newFileName;

                await WriteOk(http, new { url = fileUrl });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Upload error: {ex}", 500);
            }
        }
    }
}
