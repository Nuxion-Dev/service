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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        builder.Services.AddControllers();

        ILauncher.Load();
        //ILauncher.Launch("Bopl Battle");
        
        var host = builder.Build();
        host.MapControllers();
        host.UseCors("AllowAnyOrigin");
        host.Run();
    }
}