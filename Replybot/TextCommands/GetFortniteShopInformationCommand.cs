using DiscordDotNetUtilities.Interfaces;
using Fortnite_API.Objects.V2;
using Replybot.Models;
using Replybot.ServiceLayer;

namespace Replybot.TextCommands
{
    public class GetFortniteShopInformationCommand
    {
        private readonly FortniteApi _fortniteApi;
        private readonly DiscordSettings _discordSettings;
        private readonly IDiscordFormatter _discordFormatter;

        private const string MoreText = "\n(see shop for more)";

        public GetFortniteShopInformationCommand(
            FortniteApi fortniteApi,
            DiscordSettings discordSettings,
            IDiscordFormatter discordFormatter)
        {
            _fortniteApi = fortniteApi;
            _discordSettings = discordSettings;
            _discordFormatter = discordFormatter;
        }

        public async Task<Embed?> GetFortniteShopInformationEmbed(SocketMessage message)
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

        private EmbedFieldBuilder BuildShopSection(BrShopV2StoreFront featured, string defaultSectionName)
        {
            var featuredItems = new HashSet<string>();
            foreach (var section in featured.Entries)
            {
                var sectionTextToAdd = section.IsBundle
                    ? $"{BuildBundleField(section)}"
                    : $"{BuildSectionField(section)}";

                if (MakeFeaturedItemsString(featuredItems).Length + sectionTextToAdd.Length >= _discordSettings.MaxCharacters - MoreText.Length)
                {
                    featuredItems.Add(MoreText);
                    break;
                }

                featuredItems.Add(sectionTextToAdd);
            }

            var outputString = MakeFeaturedItemsString(featuredItems);

            return new EmbedFieldBuilder
            {
                Name = featured.HasName ? featured.Name : defaultSectionName,
                Value = outputString,
                IsInline = false
            };
        }

        private static string MakeFeaturedItemsString(HashSet<string> featuredItems)
        {
            return string.Join("\n", featuredItems);
        }

        private string BuildSectionField(BrShopV2StoreFrontEntry section)
        {
            var priceDisplay = BuildPriceDisplay(section.FinalPrice, section.RegularPrice);
            var itemToUse = section.Items.FirstOrDefault();
            return $"{itemToUse?.Name} ({itemToUse?.Type.DisplayValue}) - {priceDisplay}";
        }

        private string BuildBundleField(BrShopV2StoreFrontEntry bundle)
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
}
