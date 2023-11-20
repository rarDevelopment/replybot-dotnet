using System.Text.Json;
using Replybot.Models;
using System.Net.Http.Json;
using Replybot.Models.SchemaModels;

namespace Replybot.ServiceLayer;

public class DefaultRepliesService(IHttpClientFactory httpClientFactory)
{
    public async Task<IList<GuildReplyDefinition>?> GetDefaultReplies()
    {
        var client = httpClientFactory.CreateClient(HttpClients.DefaultReplies.ToString());
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