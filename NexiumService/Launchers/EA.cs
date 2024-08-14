using System.Diagnostics;
using Microsoft.Win32;
using NexiumService.Modules;

namespace NexiumService.Launchers;

public class EA : ILauncher
{
    public string GetLauncherLocation()
    {
        var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Electronic Arts\EA Desktop", false);
        if (reg != null)
        {
            var path = reg.GetValue("LauncherAppPath") as string;
            if (path != null)
            {
                return path;
            }
        }

        return string.Empty;
    }

    public string GetLauncherName()
    {
        return "Electronic Arts";
    }

    public void DetectGames()
    { 
        var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Origin Games", false);
        if (reg == null) return;
        
        var games = reg.GetSubKeyNames();
        List<GameInfo> tempGames = new();
        foreach (var game in games)
        {
            var gameReg = reg.OpenSubKey(game.Replace(@"HKEY_LOCAL_MACHINE\", ""), false);
            string displayName = gameReg.GetValue("DisplayName") as string;
            
            tempGames.Add(new GameInfo
            {
                Name = displayName,
                DisplayName = displayName,
                BannerImage = null,
                LastPlayed = 0,
                ShortcutSlot = -1,
                LauncherName = GetLauncherName(),
                GameId = game,
                Favourite = false,
                LauncherLocation = GetLauncherLocation(),
                GameDir = gameReg.GetValue("InstallDir") as string,
                ExeFile = gameReg.GetValue("InstallDir") as string
            });
        }
    }

    public void LaunchGame(GameInfo info)
    {
        throw new NotImplementedException();
    }
}