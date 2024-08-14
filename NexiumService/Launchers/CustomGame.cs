using System.Diagnostics;
using System.Text.Json.Nodes;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public class CustomGame : ILauncher
{
    public string GetLauncherName()
    {
        return "Custom Game";
    }
    
    public string GetLauncherLocation()
    {
        return "null";
    }
    
    public void DetectGames()
    {
        // pass
    }
    
    public void LaunchGame(GameInfo info)
    {
        var exe = info.ExeFile;
        var args = info.LaunchArgs;
        
        var process = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        Process.Start(process);
    }
    
    public static GameInfo AddGame(string name, string? banner, string exe, string args)
    {
       var info = new GameInfo
        {
            DisplayName = name,
            BannerImage = banner,
            LastPlayed = 0,
            ShortcutSlot = -1,
            LauncherName = "Custom Game",
            GameId = Guid.NewGuid().ToString(),
            Favourite = false,
            LauncherLocation = "null",
            GameDir = Path.GetDirectoryName(exe),
            ExeFile = Path.GetFileName(exe),
            LaunchCommand = exe,
            LaunchArgs = args
        };
       
       
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        var games = obj["games"].AsArray();
        
        ILauncher.InstalledGames.Add(info);
        games.Add(JsonNode.Parse(info.ToJson()));
        
        obj["games"] = games;
        storage.Write(obj);
        return info;
    }
}