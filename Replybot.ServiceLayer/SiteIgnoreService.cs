using Replybot.Models;

namespace Replybot.ServiceLayer
{
    public class SiteIgnoreService(IHttpClientFactory httpClientFactory)
    {
        public async Task<string?> GetSiteIgnoreList()
        {
            var client = httpClientFactory.CreateClient(HttpClients.SiteIgnoreList.ToString());
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
