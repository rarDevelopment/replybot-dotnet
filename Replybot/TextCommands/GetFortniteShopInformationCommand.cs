using DiscordDotNetUtilities.Interfaces;
using Fortnite_API.Objects.V2;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetFortniteShopInformationCommand(FortniteApi fortniteApi,
        IReplyBusinessLayer replyBusinessLayer,
        DiscordSettings discordSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string MoreText = "\n(see shop for more)";
    private const string SectionSeparator = "\n";
    private readonly string[] _triggers = ["fortnite shop"];

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned && _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        return Task.FromResult(new CommandResponse
        {
            Description = "The Fortnite API deprecated this endpoint. Maybe it will return in the future!",
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = false
        });
        //var embed = await GetFortniteShopInformationEmbed(message);

        //return new CommandResponse
        //{
        //    Embed = embed,
        //    Reactions = null,
        //    StopProcessing = true,
        //    NotifyWhenReplying = false
        //};
    }

    private async Task<Embed?> GetFortniteShopInformationEmbed(SocketMessage message)
    {
        var shopInfo = await fortniteApi.GetFortniteShopInformation();
        if (shopInfo == null)
        {
            return null;
        }

        var date = shopInfo.Date;
        var shopItems = new List<EmbedFieldBuilder>();

        if (shopInfo.HasFeatured)
        {
            shopItems.Add(BuildShopSection(shopInfo.Featured, "Featured"));
        }

        if (shopInfo.HasDaily)
        {
            BuildShopSection(shopInfo.Daily, "Daily");
        }

        if (!shopItems.Any())
        {
            return discordFormatter.BuildRegularEmbedWithUserFooter(
                $"Fortnite Shop Information - {date.ToShortDateString()}",
                "No shop items could be found for today.",
                message.Author
            );
        }

        return discordFormatter.BuildRegularEmbedWithUserFooter(
            $"Fortnite Shop Information - {date.ToShortDateString()}",
            "",
            message.Author,
            shopItems
        );
    }

    private EmbedFieldBuilder BuildShopSection(BrShopV2StoreFront storeSection, string defaultSectionName)
    {
        var storeItems = new HashSet<string>();
        foreach (var section in storeSection.Entries)
        {
            var sectionTextToAdd = section.IsBundle
                ? $"{BuildBundleField(section)}"
                : $"{BuildSectionField(section)}";

            var lengthIfAdded = MakeShopItemsString(storeItems).Length + sectionTextToAdd.Length;
            var maxAllowedCharacters = discordSettings.MaxCharacters - (MoreText.Length + SectionSeparator.Length); //extra SectionSeparator to account for when MoreText is joined in with the others down below
            if (lengthIfAdded >= maxAllowedCharacters)
            {
                storeItems.Add(MoreText);
                break;
            }

            storeItems.Add(sectionTextToAdd);
        }

        var outputString = MakeShopItemsString(storeItems);
        try
        {
            return new EmbedFieldBuilder
            {
                Name = storeSection.HasName ? storeSection.Name : defaultSectionName,
                Value = outputString,
                IsInline = false
            };
        }
        catch (Exception ex)
        {
            if (ex.Message.ToLower().Contains("field value length must be less than or equal to 1024"))
            {
                return new EmbedFieldBuilder
                {
                    Name = storeSection.HasName ? storeSection.Name : defaultSectionName,
                    Value = outputString[..discordSettings.MaxCharacters],
                    IsInline = false
                };
            }

            logger.LogError(ex, "Error in GetFortniteShopInformationCommand");
            return new EmbedFieldBuilder
            {
                Name = "Error Loading Section",
                Value = "There was an error loading this section.",
                IsInline = false
            };
        }
    }

    private static string MakeShopItemsString(IEnumerable<string> shopItems)
    {
        return string.Join(SectionSeparator, shopItems);
    }

    private static string BuildSectionField(BrShopV2StoreFrontEntry section)
    {
        var priceDisplay = BuildPriceDisplay(section.FinalPrice, section.RegularPrice);
        var itemToUse = section.Items.FirstOrDefault();
        return $"{itemToUse?.Name} ({itemToUse?.Type.DisplayValue}) - {priceDisplay}";
    }

    private static string BuildBundleField(BrShopV2StoreFrontEntry bundle)
    {
        var priceDisplay = BuildPriceDisplay(bundle.FinalPrice, bundle.RegularPrice);
        return $"{bundle.Bundle.Name} - {priceDisplay}";
    }

    private static string BuildPriceDisplay(int finalPrice, int regularPrice)
    {
        var priceDisplay = finalPrice != regularPrice
            ? $"~~{regularPrice}~~ {finalPrice}"
            : finalPrice.ToString();
        return priceDisplay;
    }
}