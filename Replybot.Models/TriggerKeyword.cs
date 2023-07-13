namespace Replybot.Models;

public enum TriggerKeyword
{
    Username,
    UserTag,
    VersionNumber,
    Message,
    MessageUpperCase,
    DeleteMessage,
    MessageWithoutReplybot,
    MessageSpongebob,
    MessageEncoded,
    MessageEncodedWithoutTrigger,
    MessageWithoutTrigger,
    MentionedUserAvatar,
    ServerIcon,
    ServerBanner,
    MemberCount,
    ChannelCreateDate,
    BotName,
    Anything,

    FixTwitter,
    BreakTwitter,
    FixInstagram,
    BreakInstagram,
    FixBluesky
}