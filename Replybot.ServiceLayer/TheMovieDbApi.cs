using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace Replybot.ServiceLayer;

public class TheMovieDbApi
{
    private readonly TMDbClient _tmDbClient;

    public TheMovieDbApi(TMDbClient tmDbClient)
    {
        _tmDbClient = tmDbClient;
    }

    public async Task<SearchContainer<SearchMovie>> SearchMovies(string searchTerm)
    {
        return await _tmDbClient.SearchMovieAsync(searchTerm, includeAdult: false);
    }

    public async Task<Movie> GetMovie(int id)
    {
        return await _tmDbClient.GetMovieAsync(id, MovieMethods.Credits);
    }
}