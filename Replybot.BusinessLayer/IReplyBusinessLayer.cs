using Discord;
using Discord.WebSocket;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public interface IReplyBusinessLayer
{
    Task<GuildReplyDefinition?> GetReplyDefinition(string message, string? guildId, string? channelId = null, string? userId = null);
    bool IsBotNameMentioned(SocketMessage message, ulong botUserId, IReadOnlyCollection<IGuildUser> guildUsers);
    bool GetWordMatch(string triggerTerm, string input);
}