namespace Replybot.Events;

public class GuildMemberUpdatedEventHandler
{
    public async Task HandleEvent(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
    {
        if (!cachedOldUser.HasValue)
        {
            return;
        }

        var oldUser = cachedOldUser.Value;

        var announceChange = true; // TODO: get from db
        var tagUserInChange = true; // TODO: get from db

        if (!announceChange)
        {
            return;
        }
        if (newUser.GuildAvatarId != oldUser.GuildAvatarId)
        {
            var avatarUrl = newUser.GetGuildAvatarUrl(ImageFormat.Jpeg);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = newUser.GetDisplayAvatarUrl(ImageFormat.Jpeg);
            }
            await newUser.Guild.SystemChannel.SendMessageAsync(
                $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look! Check it out: {avatarUrl}");
        }
    }
}