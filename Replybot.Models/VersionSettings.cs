namespace Replybot.Models;

public class VersionSettings
{
    public VersionSettings(string versionNumber)
    {
        VersionNumber = versionNumber;
    }

    public string VersionNumber { get; set; }
}