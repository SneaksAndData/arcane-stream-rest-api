using System;
using System.Threading.Tasks;
using Akka.Util;
using Arcane.Framework.Contracts;
using Arcane.Framework.Services.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Arcane.Stream.RestApi;

public static class Hosting
{
    
    public static async Task<int> RunStream(this IHost host, ILogger logger, Func<Exception, Task<Option<int>>> handleUnknownException = null)
    {
        var runner = host.Services.GetRequiredService<IStreamRunnerService>();
        var context = host.Services.GetRequiredService<IStreamContext>();
        var lifetimeService = host.Services.GetRequiredService<IStreamLifetimeService>();
        var graphBuilder = host.Services.GetRequiredService<IStreamGraphBuilder<IStreamContext>>();
        try
        {
            var completeTask = runner.RunStream(() => graphBuilder.BuildGraph(context));
            await completeTask;
        }
        catch (Exception e)
        {
                if (handleUnknownException is null)
                {
                    return FatalExit(e, logger);
                }

                return await handleUnknownException(e) switch
                {
                    { HasValue: true, Value: var exitCode } => exitCode,
                    _ => FatalExit(e, logger),
                };

        }
        finally
        {
            lifetimeService.Dispose();
        }

        logger.Information("Streaming job is completed successfully, exiting");
        return ExitCodes.SUCCESS;
    }

    private static int FatalExit(Exception e, ILogger logger)
    {
        logger.Error(e, "Unhandled exception occurred");
        return ExitCodes.FATAL;
    }
}
