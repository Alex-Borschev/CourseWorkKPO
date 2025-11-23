// FileLogger.cs
// Утилита для логирования подключений и простых сообщений в файл.
// Изменения:
// - Вынес логи в отдельный класс, чтобы убрать лок и File.AppendAllText из TcpServer.
// - Логирование в файл централизовано (можно расширить — ротация, размеры и т.д.).

using System;
using System.IO;

namespace Server
{
    public static class FileLogger
    {
        private const string LOG_FILE = "connections.log";
        private static readonly object logLock = new object();

        public static void LogConnection(string ip, string time, int threadId)
        {
            lock (logLock)
            {
                string logEntry = $"{time} | IP: {ip} | Поток: {threadId}";
                try
                {
                    File.AppendAllText(LOG_FILE, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FileLogger error: " + ex.Message);
                }
                Console.WriteLine("Логирование подключения: " + logEntry);
            }
        }

        // Можно расширить: LogInfo, LogError и т.д.
    }
}
