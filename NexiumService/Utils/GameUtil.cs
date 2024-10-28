using System.Diagnostics;
using NexiumService.Launchers;
using NexiumService.Modules;

namespace NexiumService.Utils;

public class GameUtil
{

    public static bool IsGameRunning(GameInfo info)
    {
        string exe = info.ExeFile;
        string name = info.Name;
        foreach (var process in Process.GetProcessesByName(info.Name))
        {
            Console.WriteLine(process.ProcessName + " - " + process.MainWindowTitle);
            if (process.ProcessName == name || process.MainWindowTitle.Contains(name))
            {
                Console.WriteLine("Game is running");
                return true;
            }
        }
        
        return false;
    }

    public static void CheckRunningGames()
    {
        List<GameInfo> runningGames = new();
        foreach (var game in ILauncher.GetRunningGames())
        {
            runningGames.Add(game);
        }
        
        foreach (var game in ILauncher.GetInstalledGames())
        {
            if (IsGameRunning(game))
            {
                runningGames.Add(game);
            }
            else if (runningGames.Contains(game))
            {
                runningGames.Remove(game);
            }
        }
        
        ILauncher.SetRunningGames(runningGames);
    }
    
}