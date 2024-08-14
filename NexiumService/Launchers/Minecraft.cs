using System.Diagnostics;
using System.Text.Json.Nodes;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public class Minecraft : ILauncher
{
    public string GetLauncherName()
    {
        return "Minecraft";
    }

    public string GetLauncherLocation()
    {
        var output = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = "/C reg query HKEY_CLASSES_ROOT\\Applications\\MinecraftLauncher.exe"
        };
        var process = Process.Start(output);
        if (process == null) return null;

        process.WaitForExit();
        var error = process.StandardError.ReadToEnd();
        if (error.Length != 0)
        {
            output = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList =
                {
                    "/C",
                    "Reg",
                    "query",
                    "HKEY_CURRENT_USER\\Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages",
                    "/s",
                    "/f",
                    "Microsoft.4297127D64EC6*"
                }
            };
            process = Process.Start(output);
            if (process == null) return null;
            
            process.WaitForExit();
            string result = process.StandardOutput.ReadToEnd();
            if (result.Contains("0 matching")) return null;
            
            string[] lines = result.Split('\n');
            string line = lines.First(x => x.Trim().StartsWith("PackageRootFolder"));
            string path = line.Trim().Split("    ")[2].Trim();
            return path;
        }
        else
        {
            string result = process.StandardOutput.ReadToEnd();
            return result.Split('\n')[2].Split("    ")[1].Trim();
        }
    }

    public void DetectGames()
    {
        string path = GetLauncherLocation();
        bool any = ILauncher.InstalledGames.Any(x => x.GameId == "Minecraft");
        if (path == null)
        {
            if (any)
            {
                ILauncher.InstalledGames.Remove(ILauncher.InstalledGames.Find(x => x.GameId == "Minecraft"));
            }

            return;
        }
        if (any) return;

        Banner banner = new();
        GameInfo info = new GameInfo {
            Name = "Minecraft",
            DisplayName = "Minecraft",
            BannerImage = banner.FetchBanner("Minecraft", "Minecraft", "Minecraft"),
            LastPlayed = 0,
            LauncherName = "Minecraft",
            ShortcutSlot = -1,
            GameSize = 0,
            GameId = "Minecraft",
            Favourite = false,
            LauncherLocation = path,
            GameDir = path,
            ExeFile = "Minecraft.exe"
        };
        
        ILauncher.InstalledGames.Add(info);
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        var games = obj["games"].AsArray();
        games.Add(JsonNode.Parse(info.ToJson()));
        obj["games"] = games;
        
        storage.Write(obj);
    }
    
    public void LaunchGame(GameInfo info)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = info.GameDir + "\\" + info.ExeFile,
            Arguments = info.LaunchArgs,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        Process? process = Process.Start(processInfo);
        if (process == null) return;
        
        ILauncher.RunningGames.Add(info);
        process.WaitForExitAsync().ContinueWith(_ =>
        {
            ILauncher.RunningGames.Remove(info);
        });
    }
}