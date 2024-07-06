﻿using System.Text.Json;

namespace NexiumService.Modules;

public struct GameInfo
{
    public string DisplayName { get; set; }
    public string BannerImage { get; set; }
    public long LastPlayed { get; set; }
    public bool Favourite { get; set; }
    
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
        return JsonSerializer.Serialize(this);
    }
}