using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace Server.Handlers
{
    public class CategoriesGetHandler : CommandHandler
    {
        public override string Command => "GET/api/categories";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                var terms = context.Db.GetAllTerms();

                var categories = terms
                    .Where(t => !string.IsNullOrEmpty(t.category))
                    .Select(t => t.category)
                    .Distinct()
                    .ToList();

                await WriteOk(http, categories);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error getting categories: {ex}", 500);
                return;
            }
        }
    }
}
