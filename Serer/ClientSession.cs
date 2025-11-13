using System;

namespace Server
{
    public class ClientSession
    {
        public string ClientAddress;
        public DateTime ConnectedAt;

        // ПОЛЕ: текущий логин авторизованного пользователя
        public string Username;

        public bool IsAuthenticated
        {
            get { return Username != null; }
        }

        public ClientSession(string address)
        {
            ClientAddress = address;
            ConnectedAt = DateTime.Now;
            Username = null;
        }
    }
}
