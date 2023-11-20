using System.Net.Http.Json;
using Replybot.Models;

namespace Replybot.ServiceLayer
{
    public class CountryConfigService(IHttpClientFactory httpClientFactory)
    {
        public async Task<IReadOnlyList<CountryConfig>?> GetCountryConfigList()
        {
            var client = httpClientFactory.CreateClient(HttpClients.CountryConfigList.ToString());
            var response = await client.GetAsync("");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var countryConfigList = await response.Content.ReadFromJsonAsync<CountryConfigList>();
            return countryConfigList?.Countries;
        }
    }
}
