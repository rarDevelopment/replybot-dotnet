using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatResponseGameData
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("game_id")]
    public int GameId { get; set; }

    [JsonPropertyName("game_name")]
    public string GameName { get; set; }

    [JsonPropertyName("game_name_date")]
    public int GameNameDate { get; set; }

    [JsonPropertyName("game_alias")]
    public string GameAlias { get; set; }

    [JsonPropertyName("game_type")]
    public string GameType { get; set; }

    [JsonPropertyName("game_image")]
    public string GameImage { get; set; }

    [JsonPropertyName("comp_lvl_combine")]
    public int CompLvlCombine { get; set; }

    [JsonPropertyName("comp_lvl_sp")]
    public int CompLvlSp { get; set; }

    [JsonPropertyName("comp_lvl_co")]
    public int CompLvlCo { get; set; }

    [JsonPropertyName("comp_lvl_mp")]
    public int CompLvlMp { get; set; }

    [JsonPropertyName("comp_lvl_spd")]
    public int CompLvlSpd { get; set; }

    [JsonPropertyName("comp_main")]
    public int CompMain { get; set; }

    [JsonPropertyName("comp_plus")]
    public int CompPlus { get; set; }

    [JsonPropertyName("comp_100")]
    public int Comp100 { get; set; }

    [JsonPropertyName("comp_all")]
    public int CompAll { get; set; }

    [JsonPropertyName("comp_main_count")]
    public int CompMainCount { get; set; }

    [JsonPropertyName("comp_plus_count")]
    public int CompPlusCount { get; set; }

    [JsonPropertyName("comp_100_count")]
    public int Comp100Count { get; set; }

    [JsonPropertyName("comp_all_count")]
    public int CompAllCount { get; set; }

    [JsonPropertyName("invested_co")]
    public int InvestedCo { get; set; }

    [JsonPropertyName("invested_mp")]
    public int InvestedMp { get; set; }

    [JsonPropertyName("invested_co_count")]
    public int InvestedCoCount { get; set; }

    [JsonPropertyName("invested_mp_count")]
    public int InvestedMpCount { get; set; }

    [JsonPropertyName("count_comp")]
    public int CountComp { get; set; }

    [JsonPropertyName("count_speedrun")]
    public int CountSpeedrun { get; set; }

    [JsonPropertyName("count_backlog")]
    public int CountBacklog { get; set; }

    [JsonPropertyName("count_review")]
    public int CountReview { get; set; }

    [JsonPropertyName("review_score")]
    public int ReviewScore { get; set; }

    [JsonPropertyName("count_playing")]
    public int CountPlaying { get; set; }

    [JsonPropertyName("count_retired")]
    public int CountRetired { get; set; }

    [JsonPropertyName("profile_dev")]
    public string ProfileDev { get; set; }

    [JsonPropertyName("profile_popular")]
    public int ProfilePopular { get; set; }

    [JsonPropertyName("profile_steam")]
    public int ProfileSteam { get; set; }

    [JsonPropertyName("profile_platform")]
    public string ProfilePlatform { get; set; }

    [JsonPropertyName("release_world")]
    public int ReleaseWorld { get; set; }
}