using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public class Steam : ILauncher
{
    private int[] _blacklistedAppId =
    {
        228980, 231350, 1493710, 1391110, 1070560, 1826330, 1113280, 1245040, 1420170, 1580130,
        1887720, 1628350, 2348590, 2180100
    };
    
    public string GetLauncherName()
    {
        return "Steam";
    }
    
    public string GetLauncherLocation()
    {
        string[] steamRegistryKeys = new string[]
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam",
        };

        string launcherLoc = null;
        foreach (string key in steamRegistryKeys)
        {
            string path = Registry.GetValue(key, "InstallPath", null)?.ToString();
            if (string.IsNullOrEmpty(path)) continue;
            
            launcherLoc = path;
            break;
        }

        Console.WriteLine($"Found Steam at {launcherLoc}");
        return launcherLoc;
    }
    
    private string[] GetLibraryFolders()
    {
        List<string> paths = new List<string>();
        string libraryFolders = GetLauncherLocation() + @"\steamapps\libraryfolders.vdf";
        if (!File.Exists(libraryFolders))
        {
            return paths.ToArray();
        }
        
        string libraryFoldersContent = File.ReadAllText(libraryFolders);
        MatchCollection matches = Regex.Matches(libraryFoldersContent, "\"path\"\\s*\"(.+?)\"");
        foreach (Match match in matches)
        {
            string path = match.Groups[1].Value;
            if (string.IsNullOrEmpty(path)) continue;
            
            paths.Add(path);
        }

        Console.WriteLine($"Found Steam libraries at {string.Join(", ", paths)}");
        return paths.ToArray();
    }
    
    public void DetectGames()
    {
        string[] steamPaths = GetLibraryFolders();
        List<GameInfo> tempGames = new List<GameInfo>();
        Banner banner = new();
        foreach (string steamPath in steamPaths) {
            string steamAppsPath = steamPath + @"\steamapps";
            
            if (!Directory.Exists(steamAppsPath))
            {
                return;
            }
            
            string[] acfFiles = Directory.GetFiles(steamAppsPath, "*.acf", SearchOption.AllDirectories);
            foreach (string acfFile in acfFiles)
            {
                string acfContent = File.ReadAllText(acfFile);
                string name = Regex.Match(acfContent, "\"name\"\\s*\"(.+?)\"").Groups[1].Value;
                string appId = Regex.Match(acfContent, "\"appid\"\\s*\"(.+?)\"").Groups[1].Value;
                if (ILauncher.InstalledGames.Any(x => x.DisplayName == name) || ILauncher.InstalledGames.Any(x => x.GameId == appId))
                {
                    continue;
                }
                
                string install_dir = Regex.Match(acfContent, "\"installdir\"\\s*\"(.+?)\"").Groups[1].Value;
                if (string.IsNullOrEmpty(name)) continue;
                if (string.IsNullOrEmpty(install_dir)) continue;
                if (string.IsNullOrEmpty(appId)) continue;
                
                if (_blacklistedAppId.Contains(int.Parse(appId))) continue;
                
                string bannerLocation = banner.FetchBanner(name, appId, "Steam");
                
                tempGames.Add(new GameInfo
                {
                    DisplayName = name,
                    BannerImage = bannerLocation,
                    LastPlayed = 0,
                    LauncherName = "Steam",
                    GameId = appId,
                    Favourite = false,
                    LauncherLocation = steamPath,
                    GameDir = steamAppsPath + @"\common\" + install_dir,
                });
            }
        }
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        var games = obj["games"].AsArray();
        foreach (GameInfo game in tempGames)
        {
            if (ILauncher.InstalledGames.Any(x => x.DisplayName == game.DisplayName) || ILauncher.InstalledGames.Any(x => x.GameId == game.GameId))
            {
                continue;
            }
            
            games.Add(JsonNode.Parse(game.ToJson()));
            ILauncher.InstalledGames.Add(game);
        }

        foreach (GameInfo game in ILauncher.InstalledGames.ToList())
        {
            if (game.LauncherName != GetLauncherName())
            {
                continue;
            }
            
            if (!Directory.Exists(game.GameDir))
            {
                string bannerImageFile = game.BannerImage;
                Storage bannerImage = new Storage(game.BannerImage);
                bannerImage.Delete();
                games.Remove(games.FirstOrDefault(x => x["GameId"].GetValue<string>() == game.GameId));
                ILauncher.InstalledGames.Remove(game);
                
            }
        }
        
        obj["games"] = games;
        storage.Write(obj);
    }

    public void LaunchGame(GameInfo info)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = GetLauncherLocation() + @"\steam.exe",
            Arguments = $"-applaunch {info.GameId} -silent" + (string.IsNullOrEmpty(info.LaunchArgs) ? "" : $" {info.LaunchArgs}"),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        
        Process.Start(processInfo);
    }
}