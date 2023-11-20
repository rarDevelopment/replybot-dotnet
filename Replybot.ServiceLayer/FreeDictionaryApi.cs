using System.Net.Http.Json;
using Replybot.Models;
using Replybot.Models.FreeDictionary;

namespace Replybot.ServiceLayer;

public class FreeDictionaryApi(IHttpClientFactory httpClientFactory)
{
    public async Task<FreeDictionaryResponse?> GetDefinition(string word)
    {
        var client = httpClientFactory.CreateClient(HttpClients.Dictionary.ToString());
        var response = await client.GetAsync(word);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var freeDictionaryResponse = await response.Content.ReadFromJsonAsync<IEnumerable<FreeDictionaryResponse>>();
        return freeDictionaryResponse?.FirstOrDefault();

    }
}