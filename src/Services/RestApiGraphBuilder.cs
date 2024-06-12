using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Dsl;
using Arcane.Framework.Services.Base;
using Arcane.Framework.Sources.RestApi;
using Arcane.Framework.Sources.RestApi.Extensions;
using Arcane.Framework.Sources.RestApi.Services.AuthenticatedMessageProviders;
using Arcane.Framework.Sources.RestApi.Services.UriProviders;
using Arcane.Stream.RestApi.Extensions;
using Arcane.Stream.RestApi.Models;
using Polly;
using Snd.Sdk.Metrics.Base;
using Snd.Sdk.Storage.Base;

namespace Arcane.Stream.RestApi.Services;

public class RestApiGraphBuilder: IStreamGraphBuilder<IStreamContext>
{
    private readonly IBlobStorageWriter blobStorageWriter;
    private readonly MetricsService metricsService;

    public RestApiGraphBuilder(IBlobStorageWriter blobStorageWriter, MetricsService metricsService)
    {
       this.blobStorageWriter = blobStorageWriter; 
       this.metricsService = metricsService;
    }
    
    public IRunnableGraph<(UniqueKillSwitch, Task)> BuildGraph(IStreamContext context)
    {
        return context switch
        {
            RestApiPagedFixedAuthStreamContext configuration => this.GetSource(configuration)
                .BuildGraph(this.metricsService,
                    this.blobStorageWriter,
                    configuration.SinkLocation,
                    context,
                    configuration.RowsPerGroup,
                    configuration.GroupingInterval),
            RestApiPagedDynamicAuthStreamContext configuration => this.GetSource(configuration)
                .BuildGraph(this.metricsService,
                    this.blobStorageWriter,
                    configuration.SinkLocation,
                    context,
                    configuration.RowsPerGroup,
                    configuration.GroupingInterval),
            RestApiFixedAuthStreamContext configuration => this.GetSource(configuration)
                .BuildGraph(this.metricsService,
                    this.blobStorageWriter,
                    configuration.SinkLocation,
                    context,
                    configuration.RowsPerGroup,
                    configuration.GroupingInterval),
            _ => throw new ArgumentOutOfRangeException($"Unsupported stream context type: {context.GetType()}")
        };
    }

    private RestApiSource GetSource(RestApiFixedAuthStreamContext context)
    {
        var queryProvider = new SimpleUriProvider(
            context.UrlTemplate,
            context.TemplatedFields.ToList(),
            context.BackFillStartDate,
            new HttpMethod(context.HttpMethod),
            context.BodyTemplate);
        var authMessage = 
            new FixedHeaderAuthenticatedMessageProvider(context.AuthHeaders);
        var rateLimitPolicy = Policy.RateLimitAsync(context.InternalRateLimitCount,
            TimeSpan.FromSeconds(context.InternalRateLimitInterval));

        return RestApiSource.Create(
            uriProvider: queryProvider,
            headerAuthenticatedMessageProvider: authMessage,
            context.IsBackfilling,
            TimeSpan.FromSeconds(context.ChangeCaptureInterval),
            TimeSpan.FromSeconds(context.LookbackInterval),
            TimeSpan.FromSeconds(context.HttpTimeout),
            context.IsBackfilling,
            rateLimitPolicy,
            context.ApiSchemaEncoded.ParseOpenApiSchema(),
            context.ResponsePropertyKeyChain);
    }

    private RestApiSource GetSource(RestApiPagedDynamicAuthStreamContext configuration)
    {
                var authMessage = (string.IsNullOrEmpty(configuration.ExpirationPeriodPropertyName),
                configuration.ExpirationPeriod.HasValue) switch
            {
                (false, _) => new DynamicBearerAuthenticatedMessageProvider(
                    tokenSource: configuration.AuthUrl,
                    tokenPropertyName: configuration.TokenPropertyName,
                    expirationPeriodPropertyName: configuration.ExpirationPeriodPropertyName,
                    requestMethod: new HttpMethod(configuration.TokenHttpMethod),
                    tokenRequestBody: configuration.TokenRequestBody),
                (true, true) => new DynamicBearerAuthenticatedMessageProvider(
                    tokenSource: configuration.AuthUrl,
                    tokenPropertyName: configuration.TokenPropertyName,
                    // ReSharper disable once PossibleInvalidOperationException
                    expirationPeriod: configuration.ExpirationPeriod.Value,
                    requestMethod: new HttpMethod(configuration.TokenHttpMethod),
                    tokenRequestBody: configuration.TokenRequestBody),
                _ => throw new ArgumentOutOfRangeException(
                    $"Unsupported combination of {nameof(RestApiPagedDynamicAuthStreamContext.ExpirationPeriodPropertyName)} and {nameof(RestApiPagedDynamicAuthStreamContext.ExpirationPeriod)}")
            };
        
        var queryProvider = new PagedUriProvider(
            configuration.UrlTemplate,
            configuration.TemplatedFields.ToList(),
            configuration.BackFillStartDate,
            new HttpMethod(configuration.HttpMethod),
            configuration.BodyTemplate).WithPageResolver(configuration.PageResolverConfiguration);
        
        var rateLimitPolicy = Policy.RateLimitAsync(configuration.InternalRateLimitCount,
            TimeSpan.FromSeconds(configuration.InternalRateLimitInterval));

        return RestApiSource.Create(
            uriProvider: queryProvider,
            authHeaderAuthenticatedMessageProvider: authMessage,
            configuration.IsBackfilling,
            TimeSpan.FromSeconds(configuration.ChangeCaptureInterval),
            TimeSpan.FromSeconds(configuration.LookbackInterval),
            TimeSpan.FromSeconds(configuration.HttpTimeout),
            configuration.IsBackfilling,
            rateLimitPolicy,
            configuration.ApiSchemaEncoded.ParseOpenApiSchema(),
            configuration.ResponsePropertyKeyChain);
    }

    private RestApiSource GetSource(RestApiPagedFixedAuthStreamContext context)
    {
        var authMessage = new FixedHeaderAuthenticatedMessageProvider(context.AuthHeaders);
        var queryProvider = new PagedUriProvider(
            context.UrlTemplate,
            context.TemplatedFields.ToList(),
            context.BackFillStartDate,
            new HttpMethod(context.HttpMethod),
            context.BodyTemplate).WithPageResolver(context.PageResolverConfiguration);
        
        var rateLimitPolicy = Policy.RateLimitAsync(context.InternalRateLimitCount,
            TimeSpan.FromSeconds(context.InternalRateLimitInterval));

        return RestApiSource.Create(
            uriProvider: queryProvider,
            headerAuthenticatedMessageProvider: authMessage,
            context.IsBackfilling,
            TimeSpan.FromSeconds(context.ChangeCaptureInterval),
            TimeSpan.FromSeconds(context.LookbackInterval),
            TimeSpan.FromSeconds(context.HttpTimeout),
            context.IsBackfilling,
            rateLimitPolicy,
            context.ApiSchemaEncoded.ParseOpenApiSchema(),
            context.ResponsePropertyKeyChain);
    }
}
