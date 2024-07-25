using System.Text.Json.Nodes;
using NexiumService.Modules;
using NexiumService.Utils;

namespace NexiumService.Launchers;

public interface ILauncher
{
    protected static List<GameInfo> InstalledGames;
    private static List<ILauncher> RegisteredLaunchers;
    
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

            string displayName = game["DisplayName"].GetValue<string>();
            string bannerImage = game["BannerImage"].GetValue<string>();
            long lastPlayed = game["LastPlayed"].GetValue<long>();
            bool favourite = game["Favourite"].GetValue<bool>();
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
                DisplayName = displayName,
                BannerImage = bannerImage,
                LastPlayed = lastPlayed,
                Favourite = favourite,
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
            info.DownloadBanner();
        }
        
        RegisteredLaunchers = new List<ILauncher>();
        RegisteredLaunchers.Add(new Steam());
        
        foreach (var launcher in RegisteredLaunchers)
        {
            launcher.DetectGames();
        }
    }
    
    public static List<GameInfo> GetInstalledGames()
    {
        return InstalledGames;
    }
    
    public static void AddCustomGame(GameInfo game)
    {
        InstalledGames.Add(game);
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
    
    public static void UpdateGame(GameInfo game)
    {
        var oldGame = InstalledGames.Find(x => x.GameId == game.GameId);
        InstalledGames.Remove(oldGame);
        InstalledGames.Add(game);
        
        var storage = new Storage("games/games.json");
        var obj = storage.Read();
        var games = obj["games"].AsArray();
        games.Remove(games.FirstOrDefault(x => x["GameId"].GetValue<string>() == game.GameId));
        games.Add(JsonNode.Parse(game.ToJson()));
        obj["games"] = games;
        storage.Write(obj);
    }
}