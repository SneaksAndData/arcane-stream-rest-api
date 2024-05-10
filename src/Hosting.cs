using System;
using System.Runtime.InteropServices;
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
        // using var lifetimeService = host.Services.GetRequiredService<IStreamLifetimeService>();
        var reg = SetupSignalHandler(runner);
        var graphBuilder = host.Services.GetRequiredService<IStreamGraphBuilder<IStreamContext>>();
        try
        {
            var completeTask = runner.RunStream(() => graphBuilder.BuildGraph(context));
            await completeTask;
        }
        catch (Exception e)
        {
                if(handleUnknownException is null)
                {
                    return FatalExit(e, logger);
                }
                return await handleUnknownException(e) switch
                {
                    {HasValue: true, Value: var exitCode} => exitCode,
                    _ => FatalExit(e, logger),
                };
        }

        logger.Information("Streaming job is completed successfully, exiting");
        return ExitCodes.SUCCESS;
    }
    
    private static PosixSignalRegistration SetupSignalHandler(IStreamRunnerService streamRunnerService,
        PosixSignal signal = PosixSignal.SIGTERM)
    {
        return PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => 
        {
            context.Cancel = true;
            Log.Information("Received a signal {signal}. Stopping the hosted stream and shutting down application", signal);
            streamRunnerService.StopStream();
        });
    }

    private static int FatalExit(Exception e, ILogger logger)
    {
        logger.Error(e, "Unhandled exception occurred");
        return ExitCodes.FATAL;
    }
}
