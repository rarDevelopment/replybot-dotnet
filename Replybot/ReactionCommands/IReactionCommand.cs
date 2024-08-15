﻿using Replybot.Models;

namespace Replybot.ReactionCommands;

public interface IReactionCommand
{
    bool CanHandle(string message, GuildConfiguration configuration);
    Task<List<Emote>> HandleReaction(SocketMessage message);
    Task<bool> IsReactingAsync(IEmote reactionEmote, GuildConfiguration guildConfiguration);
    Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser);
}