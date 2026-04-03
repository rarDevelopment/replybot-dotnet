using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Replybot.Models;
using Replybot.Models.HowLongToBeat;

namespace Replybot.ServiceLayer;

public class HowLongToBeatApi(IHttpClientFactory httpClientFactory)
{
    public async Task<HowLongToBeatResponse?> GetHowLongToBeatInformation(string searchTerm)
    {
        var hltbApiInfo = await GetHltbApiInfo();

        if (string.IsNullOrEmpty(hltbApiInfo?.UrlPath))
        {
            return null;
        }

        var client = httpClientFactory.CreateClient(nameof(HttpClients.HowLongToBeat));

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var authResponse = await client.GetAsync($"/api/{hltbApiInfo.UrlPath}/init?t={timestamp}");
        if (!authResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonAuthResponse = await authResponse.Content.ReadFromJsonAsync<HowLongToBeatAuthResponse>();

        if (jsonAuthResponse?.Token == null)
        {
            return null;
        }

        var hpKey = jsonAuthResponse.HpKey;
        var hpVal = jsonAuthResponse.HpVal;

        if (string.IsNullOrEmpty(hpKey) || string.IsNullOrEmpty(hpVal))
        {
            return null;
        }

        var request = new HowLongToBeatRequest
        {
            SearchType = "games",
            SearchTerms = searchTerm.Trim().Split(" "),
            SearchPage = 1,
            Size = 20,
            SearchOptions = new SearchOptions
            {
                Games = new SearchOptionsGames
                {
                    UserId = 0,
                    Platform = "",
                    SortCategory = "popular",
                    RangeCategory = "main",
                    RangeTime = new SearchOptionsGamesRangeTime
                    {
                        Min = null,
                        Max = null
                    },
                    Gameplay = new SearchOptionsGamesGameplay
                    {
                        Perspective = "",
                        Flow = "",
                        Genre = "",
                        Difficulty = "",
                    },
                    RangeYear = new SearchOptionsGamesRangeYear
                    {
                        Min = "",
                        Max = ""
                    },
                    Modifier = ""
                },
                Users = new SearchOptionsUsers
                {
                    SortCategory = "postcount"
                },
                Lists = new SearchOptionsLists
                {
                    SortCategory = "follows"
                },
                Filter = "",
                Sort = 0,
                Randomizer = 0
            }
        };

        var requestDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            JsonSerializer.Serialize(request));
        requestDict["useCache"] = JsonSerializer.Deserialize<JsonElement>("true");
        requestDict[hpKey] = JsonSerializer.Deserialize<JsonElement>($"\"{hpVal}\"");

        var content = new StringContent(JsonSerializer.Serialize(requestDict), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var httpRequest =
            new HttpRequestMessage(HttpMethod.Post, $"api/{hltbApiInfo.UrlPath}")
            {
                Content = content,
            };
        httpRequest.Headers.Add("x-auth-token", jsonAuthResponse.Token);
        httpRequest.Headers.Add("x-hp-key", hpKey);
        httpRequest.Headers.Add("x-hp-val", hpVal);

        var response = await client.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var hltbResponse = await response.Content.ReadFromJsonAsync<HowLongToBeatResponse>();
        return hltbResponse;
    }

    private async Task<HowLongToBeatApiSearchInfo?> GetHltbApiInfo()
    {
        try
        {
            var client = httpClientFactory.CreateClient(nameof(HttpClients.WebsiteApi));
            var response = await client.GetAsync("now/json/hltb");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<HowLongToBeatApiSearchInfo>();
            return json;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
