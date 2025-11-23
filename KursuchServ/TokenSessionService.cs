using System;
using System.Collections.Concurrent;

namespace Server.Auth
{
    public class TokenSessionService
    {
        // Хранилище всех активных сессий
        private readonly ConcurrentDictionary<string, SessionData> _sessions
            = new ConcurrentDictionary<string, SessionData>();

        // Время жизни токена
        private readonly TimeSpan _sessionLifetime;

        public TokenSessionService(TimeSpan sessionLifetime)
        {
            _sessionLifetime = sessionLifetime;
        }

        // -----------------------------
        // Создаёт новую сессию
        // -----------------------------
        public string CreateSession(string userId)
        {
            string token = Guid.NewGuid().ToString("N");

            var data = new SessionData
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(_sessionLifetime)
            };

            _sessions[token] = data;

            return token;
        }

        // -----------------------------
        // Проверяет токен и возвращает данные сессии
        // -----------------------------
        public SessionData ValidateToken(string token)
        {
            if (token == null) return null;

            if (_sessions.TryGetValue(token, out var session))
            {
                if (session.ExpiresAt > DateTime.UtcNow)
                {
                    // обновляем время, чтобы сессия оставалась активной
                    session.ExpiresAt = DateTime.UtcNow.Add(_sessionLifetime);
                    return session;
                }
                else
                {
                    // срок вышел — удаляем
                    _sessions.TryRemove(token, out _);
                }
            }

            return null; // токен невалидный
        }

        // -----------------------------
        // Удаление сессии
        // -----------------------------
        public void RemoveSession(string token)
        {
            if (token == null) return;
            _sessions.TryRemove(token, out _);
        }

        // -----------------------------
        // Очистка всех истёкших сессий
        // -----------------------------
        public void CleanupExpired()
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in _sessions)
            {
                if (kvp.Value.ExpiresAt <= now)
                {
                    _sessions.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    // Модель данных сессии
    public class SessionData
    {
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
