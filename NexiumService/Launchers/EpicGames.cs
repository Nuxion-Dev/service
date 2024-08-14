using System.Diagnostics;
using System.Text.Json.Nodes;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public class EpicGames : ILauncher
{
    public string GetLauncherName()
    {
        return "Epic Games";
    }

    public string GetLauncherLocation()
    {
        return @"C:\ProgramData\Epic\EpicGamesLauncher";
    }

    public void DetectGames()
    {
        string manifest = GetLauncherLocation() + @"\Data\Manifests";
        if (!Directory.Exists(manifest))
            return;
        
        var banner = new Banner();
        string[] manifests = Directory.GetFiles(manifest, "*.item", SearchOption.AllDirectories);
        List<GameInfo> tempGames = new();
        foreach (string manifestFile in manifests)
        {
            var storage = new Storage(manifestFile);
            var obj = storage.Read();
            
            string displayName = obj["DisplayName"].GetValue<string>();
            string gameId = obj["MainGameCatalogItemId"].GetValue<string>();
            string installLocation = obj["InstallLocation"].GetValue<string>();
            string? bannerLocation = banner.FetchBanner(displayName, gameId, "Epic Games");
            string exeFile = obj["LaunchExecutable"].GetValue<string>();
                
            tempGames.Add(new GameInfo
            {
                Name = displayName,
                DisplayName = displayName,
                BannerImage = bannerLocation,
                LastPlayed = 0,
                ShortcutSlot = -1,
                LauncherName = "Epic Games",
                GameId = gameId,
                Favourite = false,
                LauncherLocation = GetLauncherLocation(),
                GameDir = installLocation,
                ExeFile = exeFile,
            });
        }
        
        var gamesStorage = new Storage("games/games.json");
        var gamesObj = gamesStorage.Read();
        var games = gamesObj["games"].AsArray();
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
            if (game.LauncherName != GetLauncherName()) continue;
            
            if (!Directory.Exists(game.GameDir))
            {
                string? bannerImageFile = game.BannerImage;
                games.Remove(games.FirstOrDefault(x => x["GameId"] != null && x["GameId"].GetValue<string>() == game.GameId));
                ILauncher.InstalledGames.Remove(game);
                if (bannerImageFile == null) continue;

                Storage bannerImage = new Storage(bannerImageFile);
                bannerImage.Delete();
            }
        }
        
        gamesObj["games"] = games;
        gamesStorage.Write(gamesObj);
    }

    public void LaunchGame(GameInfo info)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = info.GameDir + @"\" + info.ExeFile,
            Arguments = info.LaunchArgs,
            WorkingDirectory = info.GameDir,
        };
        Process.Start(startInfo);
    }
}