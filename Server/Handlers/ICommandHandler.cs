using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Server
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Name of command
        /// </summary>
        string Command { get; }

        /// <summary>
        /// Command handler.
        /// </summary>
        /// <param name="payload">JSON body.</param>
        /// <param name="http">HttpContext, to provide user the answer.</param>
        /// <param name="context">Global context.</param>
        /// <param name="session">Client session.</param>
        Task Handle(JsonElement payload, HttpContext http, ServerContext context);
    }
}
