using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Handlers
{
    public class TermsGetAllHandler : CommandHandler
    {
        public override string Command => "GET/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            var terms = context.Db.GetAllTerms();
            await WriteOk(http, terms);
        }
    }
}