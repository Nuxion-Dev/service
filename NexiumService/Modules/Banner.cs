using NexiumService.Utils;

namespace NexiumService.Modules;

public class Banner
{
    public string FetchBanner(string displayName, string appId, string launcherName)
    {
        Storage storage = new($"banners/{displayName}_{launcherName}_{appId}.jpg");
        string bannerLocation = "";
        switch (launcherName)
        {
            case "Steam":
                string url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_600x900.jpg";
                storage.Download(url);
                
                bannerLocation = storage.GetPath();
                break;
        }
        
        Console.WriteLine($"Downloaded banner for {displayName} from {launcherName} to {bannerLocation}");
        return bannerLocation;
    }
}