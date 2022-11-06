namespace Replybot.Models
{
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
        BotName,
        Anything,

        AvatarAnnounceOn,
        AvatarAnnounceOff,
        AvatarMentionsOn,
        AvatarMentionsOff,
        InitConfig,
        EmbedMessage,
        HowLongToBeat,
        DefineWord,

        UserMetadata,
        GlobalMetadata
    }
}
