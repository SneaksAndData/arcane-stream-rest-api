﻿using System;
using System.Text.Json.Serialization;
using Arcane.Framework.Configuration;
using Arcane.Framework.Sources.RestApi.Models;
using Arcane.Stream.RestApi.Models.Base;

namespace Arcane.Stream.RestApi.Models;

public class RestApiFixedAuthStreamContext : RestApiFixedAuthBase
{
    /// <summary>
    /// Templated url field parameters.
    /// </summary>
    public RestApiTemplatedField[] TemplatedFields { get; init; }
    
    /// <summary>
    /// Base url template for this source.
    /// </summary>
    public string UrlTemplate { get; init; }
    
    /// <summary>
    /// Base request body template for this source.
    /// </summary>
    public string BodyTemplate { get; init; }
    
    /// <summary>
    /// Http method to be used: GET or POST.
    /// </summary>
    public string HttpMethod { get; init; }

    /// <summary>
    /// Start date for the backfill.
    /// </summary>
    [JsonConverter(typeof(UnixTimeConverter))]
    public DateTimeOffset BackFillStartDate { get; init; }

    /// <summary>
    /// Number of JsonElements per single json file.
    /// </summary>
    public int RowsPerGroup { get; init; }

    /// <summary>
    /// Max time to wait for rowsPerGroup to accumulate.
    /// </summary>
    [JsonConverter(typeof(SecondsToTimeSpanConverter))]
    [JsonPropertyName("groupingIntervalSeconds")]
    public TimeSpan GroupingInterval { get; init; }

    /// <summary>
    /// Data location for parquet files.
    /// </summary>
    public string SinkLocation { get; init; }

    /// <summary>
    /// Number of seconds to look back when determining first set of changes to extract.
    /// </summary>
    public int LookbackInterval { get; init; }

    /// <summary>
    /// Number of seconds to wait for result before timing out the http request.
    /// </summary>
    public int HttpTimeout { get; init; }

    /// <summary>
    /// How long to wait before polling for next result set.
    /// </summary>
    [JsonPropertyName("changeCaptureIntervalSeconds")]
    public int ChangeCaptureInterval { get; init; }
    
    /// <summary>
    /// Limit http request rate to this value per <see cref="InternalRateLimitInterval"/>. 
    /// </summary>
    public int InternalRateLimitCount { get; init; }
    
    /// <summary>
    /// Rate limit evaluation window.
    /// </summary>
    public int InternalRateLimitInterval { get; init; }
    
    /// <summary>
    /// Schema for the API response.
    /// </summary>
    public string ApiSchemaEncoded { get; init; }
}
