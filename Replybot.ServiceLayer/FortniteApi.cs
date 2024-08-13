using Fortnite_API;
using Fortnite_API.Objects;
using Fortnite_API.Objects.V1;
using Fortnite_API.Objects.V2;

namespace Replybot.ServiceLayer;

public class FortniteApi(FortniteApiClient client)
{
    public async Task<BrShopV2Combined?> GetFortniteShopInformation()
    {
        var shopInfo = await client.V2.Shop.GetBrCombinedAsync(GameLanguage.EN);
        return shopInfo is not { IsSuccess: true }
            ? null
            : shopInfo.Data;
    }

    public async Task<MapV1?> GetFortniteMapLocations()
    {
        var mapInfo = await client.V1.Map.GetAsync(GameLanguage.EN);
        return mapInfo is not { IsSuccess: true }
            ? null
            : mapInfo.Data;
    }
}