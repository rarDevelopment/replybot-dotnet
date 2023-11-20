namespace Replybot.Models;

public class TheMovieDbSettings(string imdbBaseUrl)
{
    public string ImdbBaseUrl { get; set; } = imdbBaseUrl;
}