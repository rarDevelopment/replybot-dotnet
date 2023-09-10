using IGDB;
using IGDB.Models;

namespace Replybot.ServiceLayer;

public class InternetGameDatabaseApi
{
    private readonly IGDBClient _igdbClient;
    public InternetGameDatabaseApi(IGDBClient igdbClient)
    {
        _igdbClient = igdbClient;
    }

    public async Task<IReadOnlyList<Game>> SearchGames(string searchTerm)
    {
        var queryFields = $"fields id,name,cover,release_dates.*,platforms.*,status,url,websites; search \"{searchTerm}\"; where version_parent = null & (category = 0  | category = 8);";
        var games = await _igdbClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, query: queryFields);
        return games.ToList();
    }
}
