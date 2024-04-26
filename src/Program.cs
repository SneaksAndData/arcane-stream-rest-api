using System;
using Arcane.Framework.Contracts;
using Arcane.Framework.Providers;
using Arcane.Framework.Providers.Hosting;
using Arcane.Stream.RestApi.Services;
using Arcane.Stream.RestApi.Streams.RestApiDynamicAuth.Models;
using Arcane.Stream.RestApi.Streams.RestApiFixedAuth.Models;
using Arcane.Stream.RestApi.Streams.RestApiPagedDynamicAuth.Models;
using Arcane.Stream.RestApi.Streams.RestApiPagedFixedAuth.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Snd.Sdk.Logs.Providers;
using Snd.Sdk.Metrics.Providers;
using Snd.Sdk.Storage.Base;
using Snd.Sdk.Storage.Providers;
using Snd.Sdk.Storage.Providers.Configurations;


Log.Logger = DefaultLoggingProvider.CreateBootstrapLogger(nameof(Arcane));
int exitCode;
try
{
    exitCode = await Host.CreateDefaultBuilder(args)
        .AddDatadogLogging()
        .ConfigureRequiredServices(services =>
        {
            return services.AddStreamGraphBuilder<RestApiGraphBuilder>(context => context.StreamKind switch
            {
                "RestApiFixedAuth" => StreamContext.ProvideFromEnvironment<RestApiFixedAuthStreamContext>(),
                "RestApiDynamicAuth" => StreamContext.ProvideFromEnvironment<RestApiDynamicAuthStreamContext>(),
                "RestApiPagedFixedAuth" => StreamContext.ProvideFromEnvironment<RestApiPagedFixedAuthStreamContext>().LoadSecrets(),
                "RestApiPagedDynamicAuth" => StreamContext.ProvideFromEnvironment<RestApiPagedDynamicAuthStreamContext>(),
                _ => throw new ArgumentOutOfRangeException($"Unknown stream kind: {context.StreamKind}")
            });
        })
     .ConfigureAdditionalServices((services, context) =>
         {
             services.AddAzureBlob(AzureStorageConfiguration.CreateDefault());
             services.AddDatadogMetrics(context.ApplicationName);
             services.AddSingleton<IBlobStorageWriter>(sp => sp.GetRequiredService<IBlobStorageService>());
         })
    .Build()
    .RunStream(Log.Logger);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return ExitCodes.FATAL;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return exitCode;

