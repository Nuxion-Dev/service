using Microsoft.AspNetCore.Mvc;
using NexiumService.Launchers;
using NexiumService.Modules;

namespace NexiumService.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    
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
    
    [HttpGet("get_banner/{gameId}")]
    public IActionResult GetBanner(string gameId)
    {
        GameInfo game = ILauncher.GetGame(gameId);
        game.DownloadBanner();
        return PhysicalFile(game.BannerImage, "image/jpeg");
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
        ILauncher.UpdateGame(game);
        return Ok(game);
    }
    
    [HttpPost("add_custom_game")]
    public IActionResult AddCustomGame([FromBody] GameInfo game)
    {
        ILauncher.AddCustomGame(game);
        return Ok(game);
    }
    
}