namespace Replybot.Models;

public class TheMovieDbSettings
{
    public TheMovieDbSettings(string imdbBaseUrl)
    {
        ImdbBaseUrl = imdbBaseUrl;
    }

    public string ImdbBaseUrl { get; set; }
}