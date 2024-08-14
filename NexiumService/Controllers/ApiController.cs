using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NexiumService.Launchers;
using NexiumService.Modules;

namespace NexiumService.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    public struct CustomGameInfo
    {
        public string Name { get; set; }
        public string? Banner { get; set; }
        public string Exe { get; set; }
        public string Args { get; set; }
    }
    
    [HttpGet]
    public string Get()
    {
        return "Well, hello there!";
    }

    [HttpGet("get_games")]
    public IActionResult GetGames()
    {
        List<GameInfo> games = ILauncher.GetInstalledGames();
        return Ok(games);
    }
    
    [HttpGet("get_games_shortcut")]
    public IActionResult GetGamesShortcut()
    {
        List<GameInfo> games = ILauncher.GetInstalledGames();
        List<GameInfo> shortcutGames = new List<GameInfo>();
        foreach (GameInfo game in games)
        {
            if (game.ShortcutSlot != -1)
                shortcutGames.Add(game);
        }
        return Ok(shortcutGames);
    }
    
    [HttpGet("get_banner/{gameId}")]
    public IActionResult GetBanner(string gameId)
    {
        GameInfo game = ILauncher.GetGame(gameId);
        string? banner = game.BannerImage;
        if (banner == null) return NoContent();
        
        return PhysicalFile(banner, "image/jpeg");
    }
    
    [HttpPost("launch_game/{gameId}")]
    public IActionResult LaunchGame(string gameId)
    {
        GameInfo game = ILauncher.GetGame(gameId);
        ILauncher launcher = ILauncher.GetLauncher(game.LauncherName);
        launcher.LaunchGame(game);
        return Ok("{\"status\": \"success\"}");
    }
    
    [HttpPost("update/{gameId}")]
    public IActionResult UpdateGame(string gameId, [FromBody] GameInfo game)
    {
        game.Save();
        return Ok(game);
    }
    
    [HttpPost("add_custom_game")]
    public IActionResult AddCustomGame([FromBody] CustomGameInfo game)
    {
        var g = CustomGame.AddGame(game.Name, game.Banner, game.Exe, game.Args);
        return Ok(g);
    }
    
    [HttpGet("refresh")]
    public IActionResult Refresh()
    {
        ILauncher.Load();
        return Ok("{\"status\": \"success\"}");
    }
    
    [HttpGet("get_running_games")]
    public IActionResult GetRunningGames()
    {
        return Ok(ILauncher.GetRunningGames());
    }
    
}