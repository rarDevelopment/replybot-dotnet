using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace Replybot.ServiceLayer;

public class TheMovieDbApi(TMDbClient tmDbClient)
{
    public async Task<SearchContainer<SearchMovie>> SearchMovies(string searchTerm)
    {
        return await tmDbClient.SearchMovieAsync(searchTerm, includeAdult: false);
    }

    public async Task<Movie> GetMovie(int id)
    {
        return await tmDbClient.GetMovieAsync(id, MovieMethods.Credits);
    }
}