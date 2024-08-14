using System.Text.Json;
using NexiumService.Launchers;
using NexiumService.Utils;

namespace NexiumService.Modules;

public struct GameInfo
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string? BannerImage { get; set; }
    public long LastPlayed { get; set; }
    public bool Favourite { get; set; }
    public bool CustomBanner { get; set; }
    public int ShortcutSlot { get; set; }
    
    public string LauncherLocation { get; set; }
    public string LauncherName { get; set; }

    public string GameId { get; set; }
    public long GameSize { get; set; }
    
    public string LaunchCommand { get; set; }
    public string LaunchArgs { get; set; }
    public string ExeFile { get; set; }
    public string GameDir { get; set; }
    
    public string ToJson()
    {
        string? json = JsonSerializer.Serialize(this);
        return json;
    }
    
    public string? DownloadBanner()
    {
        string? image = new Banner().FetchBanner(DisplayName, GameId, LauncherName);
        BannerImage = image;
        //this.Save();

        return BannerImage;
    }

    public void Save()
    {
        Console.WriteLine("Saving game info for " + DisplayName);
        ILauncher.UpdateGame(this);
    }
}