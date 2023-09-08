using Replybot.Models;

namespace Replybot.ServiceLayer
{
    public class SiteIgnoreService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SiteIgnoreService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> GetSiteIgnoreList()
        {
            var client = _httpClientFactory.CreateClient(HttpClients.SiteIgnoreList.ToString());
            var response = await client.GetAsync("");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var siteIgnoreList = await response.Content.ReadAsStringAsync();
            return siteIgnoreList;
        }
    }
}
