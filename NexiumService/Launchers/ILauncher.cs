using System.Diagnostics;
using System.Text.Json.Nodes;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public interface ILauncher
{
    protected static List<GameInfo> InstalledGames;
    protected static List<GameInfo> RunningGames = new List<GameInfo>();
    private static List<ILauncher> RegisteredLaunchers = new List<ILauncher>()
    {
        new Steam(),
        new EpicGames(),
        new Minecraft()
    };
    
    public string GetLauncherName();
    
    public string GetLauncherLocation();
    
    public void DetectGames();
    
    public void LaunchGame(GameInfo info);

    public static void Load()
    {
        InstalledGames = new List<GameInfo>();
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        if (!obj.ContainsKey("games"))
        {
            obj["games"] = new JsonArray();
            storage.Write(obj);
        }
        
        var games = obj["games"].AsArray();
        
        foreach (var game in games)
        {
            if (game == null) continue;
            if (game["DisplayName"] == null) continue;

            string name = game["Name"].GetValue<string>();
            string displayName = game["DisplayName"].GetValue<string>();
            string? bannerImage = game["BannerImage"]?.GetValue<string>();
            long lastPlayed = game["LastPlayed"].GetValue<long>();
            bool favourite = game["Favourite"].GetValue<bool>();
            int shortcutSlot = game["ShortcutSlot"].GetValue<int>();
            bool customBanner = game["CustomBanner"] == null ? false : game["CustomBanner"].GetValue<bool>();
            string launcherLocation = game["LauncherLocation"].GetValue<string>();
            string launcherName = game["LauncherName"].GetValue<string>();
            string gameId = game["GameId"].GetValue<string>();
            long gameSize = game["GameSize"].GetValue<long>();
            string launchCommand = game["LaunchCommand"] == null ? "null" : game["LaunchCommand"].GetValue<string>();
            string launchArgs = game["LaunchArgs"] == null ? "null" : game["LaunchArgs"].GetValue<string>();
            string exeFile = game["ExeFile"] == null ? "null" : game["ExeFile"].GetValue<string>();
            string gameDir = game["GameDir"].GetValue<string>();

            GameInfo info = new GameInfo
            {
                Name = name,
                DisplayName = displayName,
                BannerImage = bannerImage,
                LastPlayed = lastPlayed,
                Favourite = favourite,
                ShortcutSlot = shortcutSlot,
                CustomBanner = customBanner,
                LauncherLocation = launcherLocation,
                LauncherName = launcherName,
                GameId = gameId,
                GameSize = gameSize,
                LaunchCommand = launchCommand,
                LaunchArgs = launchArgs,
                ExeFile = exeFile,
                GameDir = gameDir
            };
            InstalledGames.Add(info);
            Thread thread = new Thread(() => info.DownloadBanner());
            thread.Start();
        }
        
        foreach (var launcher in RegisteredLaunchers)
        {
            Thread thread2 = new Thread(() => launcher.DetectGames());
            thread2.Start();
        }
        
        Thread thread3 = new Thread(() =>
        {
            Thread.Sleep(5000);
            GameUtil.CheckRunningGames();
        });
    }
    
    public static List<GameInfo> GetInstalledGames()
    {
        InstalledGames.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        return InstalledGames;
    }
    
    public static void RegisterLauncher(ILauncher launcher)
    {
        RegisteredLaunchers.Add(launcher);
    }
    
    public static ILauncher GetLauncher(string name)
    {
        return RegisteredLaunchers.Find(x => x.GetLauncherName() == name);
    }
    
    public static void Launch(string name)
    {
        var game = InstalledGames.Find(x => x.GameId == name || x.DisplayName == name);
        
        var launcher = RegisteredLaunchers.Find(x => x.GetLauncherName() == game.LauncherName);
        if (launcher == null) return;
        
        launcher.LaunchGame(game);
    }
    
    public static GameInfo GetGame(string gameId)
    {
        return InstalledGames.Find(x => x.GameId == gameId);
    }
    
    public static void UpdateGame(GameInfo possibleGame)
    {
        var game = InstalledGames.Find(x => x.GameId == possibleGame.GameId);
        InstalledGames.Remove(game);
        
        game.DisplayName = possibleGame.DisplayName;
        game.BannerImage = possibleGame.BannerImage;
        game.LastPlayed = possibleGame.LastPlayed;
        game.Favourite = possibleGame.Favourite;
        game.ShortcutSlot = possibleGame.ShortcutSlot;
        game.CustomBanner = possibleGame.CustomBanner;
        game.LauncherLocation = possibleGame.LauncherLocation;
        game.LauncherName = possibleGame.LauncherName;
        game.GameId = possibleGame.GameId;
        game.GameSize = possibleGame.GameSize;
        game.LaunchCommand = possibleGame.LaunchCommand;
        game.LaunchArgs = possibleGame.LaunchArgs;
        game.ExeFile = possibleGame.ExeFile;
        game.GameDir = possibleGame.GameDir;
        
        InstalledGames.Add(game);
        InstalledGames.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        var games = obj["games"].AsArray();
        for (int i = 0; i < games.Count; i++)
        {
            if (games[i]["GameId"].GetValue<string>() == game.GameId)
            {
                games[i] = JsonNode.Parse(game.ToJson());
            }
        }
        
        obj["games"] = games;
        storage.Write(obj);
    }
    
    public static List<GameInfo> GetRunningGames()
    {
        return RunningGames;
    }
    
    public static void SetRunningGames(List<GameInfo> info)
    {
        RunningGames.Clear();
        RunningGames.AddRange(info);
    }
    
    public static bool IsGameRunning(string gameId)
    {
        return RunningGames.Any(x => x.GameId == gameId);
    }

    /**
     * Set the shortcut slot for a game
     * @param info GameInfo object
     * @param slot Slot number, 0-2 (premium 0-5)
     * @return void
     */
    public static void SetShortcut(GameInfo info, int slot)
    {
        GameInfo? slotTaken = InstalledGames.Find(x => x.ShortcutSlot == slot);
        if (slotTaken != null)
        {
            GameInfo slotTakenCopy = (GameInfo) slotTaken;
            slotTakenCopy.ShortcutSlot = -1;
            UpdateGame(slotTakenCopy);
        }
        
        info.ShortcutSlot = slot;
        UpdateGame(info);
    }
}