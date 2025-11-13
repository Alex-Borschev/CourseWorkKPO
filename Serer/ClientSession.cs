using System;
using System.Net.Sockets;
using SharedLibrary;

namespace Server
{
    /// <summary>
    /// Представляет активное подключение клиента.
    /// Хранит его TCP-поток, состояние и авторизованного пользователя.
    /// </summary>
    public class ClientSession
    {
        public Users CurrentUser;
        public string ClientAddress;
        public DateTime ConnectedAt;

        public ClientSession(string address)
        {
            this.ClientAddress = address;
            this.ConnectedAt = DateTime.Now;
            this.CurrentUser = null;
        }

        public bool IsAuthenticated()
        {
            return CurrentUser != null;
        }
    }
}
