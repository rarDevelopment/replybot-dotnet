using DiscordDotNetUtilities.Interfaces;
using Fortnite_API.Objects.V2;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;

namespace Replybot.Commands;

public class GetFortniteShopInformationCommand : IReplyCommand
{
    private readonly FortniteApi _fortniteApi;
    private readonly DiscordSettings _discordSettings;
    private readonly KeywordHandler _keywordHandler;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    private const string MoreText = "\n(see shop for more)";
    private const string SectionSeparator = "\n";

    public GetFortniteShopInformationCommand(
        FortniteApi fortniteApi,
        DiscordSettings discordSettings,
        KeywordHandler keywordHandler,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _fortniteApi = fortniteApi;
        _discordSettings = discordSettings;
        _keywordHandler = keywordHandler;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    public bool CanHandle(string? reply)
    {
        return reply == _keywordHandler.BuildKeyword("FortniteShopInfo");
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