using System.Net.Http.Json;
using Replybot.Models;
using Replybot.Models.Bluesky;

namespace Replybot.ServiceLayer;

public class BlueskyApi(IHttpClientFactory httpClientFactory)
{
    public async Task<BlueskyRecord?> GetRecord(string repo, string rkey)
    {
        var client = httpClientFactory.CreateClient(HttpClients.Bluesky.ToString());
        var response =
            await client.GetAsync($"com.atproto.repo.getRecord?repo={repo}&collection=app.bsky.feed.post&rkey={rkey}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var blueskyRecordResponse = await response.Content.ReadFromJsonAsync<BlueskyRecord>();
        return blueskyRecordResponse;
    }

    public async Task<Stream?> GetImageOrVideo(string did, string cid)
    {
        var client = httpClientFactory.CreateClient(HttpClients.Bluesky.ToString());
        var response =
            await client.GetAsync($"com.atproto.sync.getBlob?did={did}&cid={cid}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadAsStreamAsync();
    }
}