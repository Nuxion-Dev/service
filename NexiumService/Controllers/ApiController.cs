using Microsoft.AspNetCore.Mvc;

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
        return Ok("");
    }
    
    [HttpGet("launch_game/{gameId}")]
    public IActionResult LaunchGame(string gameId)
    {
        return Ok("");
    }
    
}