using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using Replybot.ReactionCommands;

namespace Replybot.NotificationHandlers;

public class ReactionAddedNotificationHandler(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        IEnumerable<IReactionCommand> reactionCommands,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>, INotificationHandler<ReactionAddedNotification>
{
    public Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var reaction = notification.Reaction;
            var reactingUser = reaction.User.GetValueOrDefault();
            var message = await notification.Message.GetOrDownloadAsync();

            if (reactingUser is IGuildUser { IsBot: true } ||
                message == null ||
                notification.Reaction.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await configurationBusinessLayer.GetGuildConfiguration(guildChannel.Guild);
            if (config == null)
            {
                return Task.CompletedTask;
            }

            foreach (var reactionCommand in reactionCommands)
            {
                await ProcessReactions(reactionCommand, reaction, config, message, reactingUser);
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ProcessReactions(IReactionCommand reactCommand,
        IReaction reaction,
        GuildConfiguration config,
        IUserMessage message,
        IUser reactingUser)
    {
        ReactionMetadata? fixReaction = null;
        var isReacting = await reactCommand.IsReactingAsync(reaction.Emote, config);
        if (isReacting)
        {
            fixReaction = message.Reactions.FirstOrDefault(r => Equals(r.Key, reaction.Emote)).Value;
        }

        if (fixReaction == null || fixReaction.Value.ReactionCount > 2)
        {
            return;
        }

        var commandResponses = await reactCommand.HandleMessage(message, reactingUser);
        foreach (var commandResponse in commandResponses)
        {
            var allowedMentions = new AllowedMentions
            {
                AllowedTypes = AllowedMentionTypes.Users | AllowedMentionTypes.Roles | AllowedMentionTypes.Everyone,
                MentionRepliedUser = commandResponse.NotifyWhenReplying
            };

            var buttonBuilder = commandResponse.AllowDeleteButton
                ? new ComponentBuilder().WithButton("Delete This", "deleteFixedItem", emote: new Emoji("❌"))
                : null;

            if (commandResponse.FileAttachments.Any())
            {
                try
                {
                    await message.Channel.SendFilesAsync(commandResponse.FileAttachments, commandResponse.Description,
                        messageReference: new MessageReference(message.Id,
                            failIfNotExists: false),
                        allowedMentions: allowedMentions,
                        components: buttonBuilder?.Build());
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error Sending Files - {ex.Message}");
                    await message.ReplyAsync(
                        $"Sorry {reactingUser.Mention}, there was an error trying to respond to that, possibly related to the file attachment.");
                }
            }
            else
            {
                await message.ReplyAsync(commandResponse.Description, allowedMentions: allowedMentions, components: buttonBuilder?.Build(), embed: commandResponse.Embed);
            }
        }
    }

    [ComponentInteraction("deleteFixedItem")]
    public async Task DeleteButton()
    {
        try
        {
            await DeferAsync(ephemeral: true);
            if (Context.Interaction is IComponentInteraction interaction)
            {
                var mentions = interaction.Message.MentionedUserIds;
                if (mentions.FirstOrDefault() == Context.Interaction.User.Id)
                {
                    await DeleteOriginalResponseAsync();
                }
                else
                {
                    await FollowupAsync(
                        $"You can only delete that if you're the person who requested it {Context.User.Mention}!", ephemeral: true);
                    await interaction.Message.AddReactionAsync(new Emoji("❌"));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error Deleting Message Using DeleteButton - {ex.Message}");
            await FollowupAsync(
                $"Sorry {Context.User.Mention}, there was an error trying to delete that.", ephemeral: true);
        }
    }
}