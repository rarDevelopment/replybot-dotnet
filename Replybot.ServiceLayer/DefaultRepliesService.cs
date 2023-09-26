using System.Text.Json;
using Replybot.DataLayer.SchemaModels;
using Replybot.Models;
using System.Net.Http.Json;

namespace Replybot.ServiceLayer
{
    public class DefaultRepliesService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DefaultRepliesService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IList<GuildReplyDefinition>?> GetDefaultReplies()
        {
            var client = _httpClientFactory.CreateClient(HttpClients.DefaultReplies.ToString());
            var response = await client.GetAsync("");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var defaultReplyData = await response.Content.ReadFromJsonAsync<DefaultReplyData>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return defaultReplyData?.DefaultReplies.Select(tr => tr.ToDomain()).ToList();
        }
    }
}
