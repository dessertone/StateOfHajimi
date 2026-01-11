using Avalonia;
using System;
using Serilog;
using StateOfHajimi.Client.Models.Enums;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Client;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/game-log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        try
        {
            Log.Information("Loading game settings...");
            GameConfig.Initialize();
            AttributeHelper.Initialize();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            Log.Information("Game start...");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fetal error has occured, game will now exit.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}