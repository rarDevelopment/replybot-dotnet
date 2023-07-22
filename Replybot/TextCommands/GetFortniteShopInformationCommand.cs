using DiscordDotNetUtilities.Interfaces;
using Fortnite_API.Objects.V2;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetFortniteShopInformationCommand : ITextCommand
{
    private readonly FortniteApi _fortniteApi;
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly DiscordSettings _discordSettings;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    private const string MoreText = "\n(see shop for more)";
    private const string SectionSeparator = "\n";
    private readonly string[] _triggers = { "fortnite shop" };

    public GetFortniteShopInformationCommand(
        FortniteApi fortniteApi,
        IReplyBusinessLayer replyBusinessLayer,
        DiscordSettings discordSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _fortniteApi = fortniteApi;
        _replyBusinessLayer = replyBusinessLayer;
        _discordSettings = discordSettings;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned && _triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var embed = await GetFortniteShopInformationEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true
        };
    }

    private async Task<Embed?> GetFortniteShopInformationEmbed(SocketMessage message)
    {
        var shopInfo = await _fortniteApi.GetFortniteShopInformation();
        if (shopInfo == null)
        {
            return null;
        }

        var date = shopInfo.Date;
        var shopItems = new List<EmbedFieldBuilder>
        {
            BuildShopSection(shopInfo.Featured, "Featured"),
            BuildShopSection(shopInfo.Daily, "Daily")
        };

        return _discordFormatter.BuildRegularEmbed(
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
            var maxAllowedCharacters = _discordSettings.MaxCharacters - (MoreText.Length + SectionSeparator.Length); //extra SectionSeparator to account for when MoreText is joined in with the others down below
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
                    Value = outputString[.._discordSettings.MaxCharacters],
                    IsInline = false
                };
            }

            _logger.LogError(ex, "Error in GetFortniteShopInformationCommand");
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