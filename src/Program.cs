using System;
using System.Reflection;
using Arcane.Framework.Contracts;
using Arcane.Framework.Providers;
using Arcane.Framework.Providers.Hosting;
using Arcane.Stream.RestApi.Extensions;
using Arcane.Stream.RestApi.Models;
using Arcane.Stream.RestApi.Models.Base;
using Arcane.Stream.RestApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Snd.Sdk.Logs.Providers;
using Snd.Sdk.Metrics.Configurations;
using Snd.Sdk.Metrics.Providers;
using Snd.Sdk.Storage.Providers;
using Snd.Sdk.Storage.Providers.Configurations;

Log.Logger = DefaultLoggingProvider.CreateBootstrapLogger(nameof(Arcane));

int exitCode;
try
{
    exitCode = await Host.CreateDefaultBuilder(args)
        .AddDatadogLogging((_, _, configuration) => configuration.WriteTo.Console())
        .ConfigureRequiredServices(services =>
        {
            return services.AddStreamGraphBuilder<RestApiGraphBuilder>(context =>
            {
                // Dynamically load the stream context based on the stream kind
                // This logic is used to determine the stream context type based on the stream kind
                // Until the StreamComponent is implemented, this .
                var contextNS = typeof(RestApiFixedAuthStreamContext).Namespace;
                var typeFullName = $"{contextNS}.{context.StreamKind}StreamContext";
                var targetType = Assembly.GetExecutingAssembly().GetType(typeFullName);
                if (targetType is null)
                {
                    throw new ArgumentException($"Unknown stream kind {typeFullName}. Cannot load stream context.");
                }

                return ((RestApiStreamContextBase)StreamContext.ProvideFromEnvironment(targetType)).LoadSecrets();
            });
        })
        .ConfigureAdditionalServices((services, context) =>
        {
            services.AddAzureBlob(AzureStorageConfiguration.CreateDefault());
            services.AddDatadogMetrics(configuration: DatadogConfiguration.UnixDomainSocket(context.ApplicationName));
            services.AddSingleton(sp => sp.GetBlobStorageWriter());
            services.AddAwsS3Writer(AmazonStorageConfiguration.CreateFromEnv());
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
