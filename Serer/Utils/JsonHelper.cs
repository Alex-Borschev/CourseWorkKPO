// JsonHelper.cs
// Утилиты для безопасного чтения/записи JSON-файлов.
// Изменения:
// - Вынес повторяющийся код сериализации/десериализации в один класс.
// - Унифицировал опции сериализации (WriteIndented и PropertyNameCaseInsensitive).
// - Обрабатывает отсутствие файла и ошибки чтения/парсинга.

using System;
using System.IO;
using System.Text.Json;

namespace Server
{
    public static class JsonHelper
    {
        public static T ReadJsonFile<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine("JsonHelper.ReadJsonFile error: " + ex.Message);
                return null;
            }
        }

        public static bool WriteJsonFile<T>(string path, T obj)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(obj, options);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("JsonHelper.WriteJsonFile error: " + ex.Message);
                return false;
            }
        }
    }
}
