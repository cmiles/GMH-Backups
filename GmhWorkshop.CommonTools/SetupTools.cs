using PointlessWaymarks.CommonTools;
using PointlessWaymarks.VaultfuscationTools;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Extensions.Logging;

namespace GmhWorkshop.CommonTools;

public class SetupReturn
{
    public string SettingsFile { get; set; } = string.Empty;
    public bool Interactive { get; set; }
    public bool PromptAsIfNewFile { get; set; }
}

public static class SetupTools
{
    public static async Task<SetupReturn> SetupAndGetSettingsFile(string[] args)
    {
        var returnSettings = new SetupReturn();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithCallerInfo(true,
                "GmhWorkshop.",
                "gmhworkshop")
            .Enrich
            .FromGlobalLogContext()
            .MinimumLevel.Verbose()
            .LogToConsole()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            Console.WriteLine();
            Console.WriteLine("Fatal Unhandled Exception");
            Console.WriteLine();
            Console.WriteLine(eventArgs.ToString());

            Log.Fatal(eventArgs.ExceptionObject as Exception,
                $"Unhandled Exception {(eventArgs.ExceptionObject as Exception)?.Message ?? ""}");
            Log.CloseAndFlush();
        };

        if (args.Length <= 1)
        {
            Console.WriteLine("You must provide the name for the settings file to use - this can be an existing settings file or a new file to create.");

            Log.Error(
                $"The Settings File must be provided first argument to this program (found {args.Length} arguments)");
            await Log.CloseAndFlushAsync();
            return returnSettings;
        }

        returnSettings.SettingsFile = args[0].Trim();

        returnSettings.Interactive = !args.Any(x => x.Contains("-notinteractive", StringComparison.OrdinalIgnoreCase));
        returnSettings.PromptAsIfNewFile = args.Any(x => x.Contains("-redo", StringComparison.OrdinalIgnoreCase));

        return returnSettings;
    }
}
