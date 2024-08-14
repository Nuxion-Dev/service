using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using NexiumService.Utils;

namespace NexiumService.Modules;

public class Banner
{
    public static String DefaultBanner = new Storage("default_banner.jpg").GetPath();
    
    public string? FetchBanner(string displayName, string appId, string launcherName)
    {
        Storage storage = new($"banners/{displayName}_{launcherName}_{appId}.jpg");
        string? bannerLocation = null;
        switch (launcherName)
        {
            case "Steam":
                string url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_600x900.jpg";
                storage.Download(url);
                
                bannerLocation = storage.GetPath();
                break;
            case "Epic Games":
                bannerLocation = RawgFetchBanner(displayName, appId, launcherName);
                break;
            case "Minecraft":
                url = "https://cdn.grcq.dev/3db40d2c.png";
                storage.Download(url);
                
                bannerLocation = storage.GetPath();
                break;
        }

        if (!storage.Exists())
        {
            return null;
        }
        
        Console.WriteLine($"Downloaded banner for {displayName} from {launcherName} to {bannerLocation}");
        return bannerLocation;
    }

    private string? RawgFetchBanner(string displayName, string appId, string launcherName)
    {
        Storage storage = new($"banners/{displayName}_{launcherName}_{appId}.jpg");
        string url =
            $"https://api.rawg.io/api/games?key=67c5fa697fc94ac3bfc672de97fe8902&search={displayName.Replace(" ", "-").Replace("_", "")}&search_precise&search_exact&stores=11";
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(url).Result;
            if (response.StatusCode != HttpStatusCode.OK) return null;

            var content = response.Content.ReadAsStringAsync().Result;
            var obj = JsonNode.Parse(content).AsObject();
            if (!obj.ContainsKey("results")) return null;
            
            var results = obj["results"].AsArray();
            if (results.Count == 0) return null;
            
            var game = results[0].AsObject();
            if (!game.ContainsKey("background_image")) return null;
            
            string bannerUrl = game["background_image"].GetValue<string>();
            storage.Download(bannerUrl);
        }
        
        return storage.GetPath();
    }
}