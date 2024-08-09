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
                        Genre = ""
                    },
                    Modifier = ""
                },
                Users = new SearchOptionsUsers
                {
                    SortCategory = "postcount"
                },
                Filter = "",
                Sort = 0,
                Randomizer = 0
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/search/5683ebd079f1c360")
        {
            Content = content
        };

        var response = await client.SendAsync(httpRequest);
        if (response.IsSuccessStatusCode)
        {
            var hltbResponse = await response.Content.ReadFromJsonAsync<HowLongToBeatResponse>();
            return hltbResponse;
        }

        return null;
    }
}