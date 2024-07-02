using System;
using System.Text.Json;
using Arcane.Framework.Services.Base;

namespace Arcane.Stream.RestApi.Models.Base;

/// <summary>
/// Base class for REST Api streaming configurations
/// </summary>
public abstract class RestApiStreamContextBase: IStreamContext, IStreamContextWriter
{
    /// <inheritdoc cref="IStreamContext.StreamId"/>>
    public string StreamId { get; private set; }
    
    /// <inheritdoc cref="IStreamContext.StreamKind"/>>
    public string StreamKind { get; private set; }
    
    /// <inheritdoc cref="IStreamContext.IsBackfilling"/>>
    public bool IsBackfilling { get; private set; }

    /// <summary>
    /// Data location for the sink.
    /// </summary>
    public string SinkLocation { get; init; }

    /// <inheritdoc cref="IStreamContextWriter.SetStreamId"/>>
    public void SetStreamId(string streamId)
    {
        this.StreamId = streamId;
    }

    /// <inheritdoc cref="IStreamContextWriter.SetBackfilling"/>>
    public void SetBackfilling(bool isRunningInBackfillMode)
    {
        this.IsBackfilling = isRunningInBackfillMode;
    }

    /// <inheritdoc cref="IStreamContextWriter.SetStreamKind"/>>
    public void SetStreamKind(string streamKind)
    {
        this.StreamKind = streamKind;
    }

    public abstract RestApiStreamContextBase LoadSecrets();

    protected string GetSecretFromEnvironment(string secretName)
        => Environment.GetEnvironmentVariable($"{nameof(Arcane)}__{secretName}".ToUpperInvariant());

    protected TResultType GetJSONSecretFromEnvironment<TResultType>(string secretName)
        => JsonSerializer.Deserialize<TResultType>(this.GetSecretFromEnvironment(secretName) ??
                                                   throw new ArgumentNullException($"{nameof(Arcane)}__{secretName}"));
}
