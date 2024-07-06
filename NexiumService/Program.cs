using Microsoft.AspNetCore.Builder;
using NexiumService;
using NexiumService.Launchers;
using NexiumService.Modules;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHostedService<Worker>();
        builder.Services.AddWindowsService();
        builder.Services.AddControllers();
        
        ILauncher.Load();
        ILauncher.Launch("Bopl Battle");
        
        var host = builder.Build();
        host.MapControllers();
        host.Run();
    }
}