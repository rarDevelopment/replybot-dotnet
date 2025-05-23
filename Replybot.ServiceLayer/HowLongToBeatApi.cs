﻿using System.Net.Http.Headers;
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

        if (hltbApiInfo.apiSearchKey == null || hltbApiInfo.urlPath == null)
        {
            return null;
        }

        var client = httpClientFactory.CreateClient(HttpClients.HowLongToBeat.ToString());
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
                        Min = 0,
                        Max = 0
                    },
                    Gameplay = new SearchOptionsGamesGameplay
                    {
                        Perspective = "",
                        Flow = "",
                        Genre = "",
                        Difficulty = "",
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

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/{hltbApiInfo.urlPath}/{hltbApiInfo.apiSearchKey}")
        {
            Content = content
        };

        var response = await client.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var hltbResponse = await response.Content.ReadFromJsonAsync<HowLongToBeatResponse>();
        return hltbResponse;

    }

    private async Task<(string? apiSearchKey, string? urlPath)> GetHltbApiInfo()
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClients.WebsiteApi.ToString());
            var response = await client.GetAsync("now/json/hltb");
            if (!response.IsSuccessStatusCode)
            {
                return (null, null);
            }

            var json = await response.Content.ReadFromJsonAsync<HowLongToBeatApiSearchInfo>();
            return (json?.ApiSearchKey, json?.UrlPath);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }
}