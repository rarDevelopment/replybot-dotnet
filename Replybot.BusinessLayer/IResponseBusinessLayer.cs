﻿using Discord;
using Discord.WebSocket;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public interface IResponseBusinessLayer
{
    Task<TriggerResponse?> GetTriggerResponse(string message, ulong? guildId);
    Task<bool> IsBotNameMentioned(SocketMessage message, IGuild? guild, ulong botUserId);
}