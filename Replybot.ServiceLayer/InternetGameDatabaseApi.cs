using IGDB;
using IGDB.Models;

namespace Replybot.ServiceLayer;

public class InternetGameDatabaseApi(IGDBClient igdbClient)
{
    public async Task<IReadOnlyList<Game>> SearchGames(string searchTerm)
    {
        var queryFields = $"fields id,name,cover,release_dates.*,release_dates.status.*,platforms.*,status,url,websites; search \"{searchTerm}\"; where version_parent = null & (category = 0  | category = 8);";
        var games = await igdbClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, query: queryFields);
        return games.ToList();
    }
}
