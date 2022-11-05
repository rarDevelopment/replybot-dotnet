using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Replybot.Models;
using Replybot.Models.HowLongToBeat;

namespace Replybot.ServiceLayer
{
    public class HowLongToBeatApi
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HowLongToBeatApi(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HowLongToBeatResponse?> GetHowLongToBeatInformation(string searchTerm)
        {
            var client = _httpClientFactory.CreateClient(HttpClients.HowLongToBeat.ToString());
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

            //var data = JsonSerializer.Serialize(request);
            //var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            //var byteContent = new ByteArrayContent(buffer);
            //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //var content = new StringContent("{\"searchType\": \"games\",\"searchTerms\": [\"mario\"],\"searchPage\": 1,\"size\": 20,\"searchOptions\": {\"games\": {\"userId\": 0,\"platform\": \"\", \"sortCategory\": \"popular\",\"rangeCategory\": \"main\",\"rangeTime\": {\"min\": 0,\"max\": 0},\"gameplay\": {\"perspective\": \"\", \"flow\": \"\", \"genre\": \"\" }, \"modifier\": \"\" }, \"users\": {\"sortCategory\": \"postcount\"},\"filter\": \"\", \"sort\": 0,\"randomizer\": 0}}"); 
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/search")
            {
                Content = content
            };

            var response = await client.SendAsync(httpRequest);

            //var response = await client.PostAsJsonAsync("api/search", request, new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //});

            if (response.IsSuccessStatusCode)
            {
                var hltbResponse = await response.Content.ReadFromJsonAsync<HowLongToBeatResponse>();
                //    ReadAsStringAsync();
                //var jsonStr = Encoding.UTF8.GetBytes(hltbResponse);
                //var json = JsonSerializer.Deserialize<HowLongToBeatResponse>(jsonStr);
                return hltbResponse;
            }

            return null;
        }
    }
}
