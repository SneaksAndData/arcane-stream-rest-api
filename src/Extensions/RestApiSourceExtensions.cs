using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Dsl;
using Arcane.Framework.Contracts;
using Arcane.Framework.Services.Base;
using Arcane.Framework.Sinks.Json;
using Arcane.Framework.Sources.RestApi;
using Parquet.Data;
using Snd.Sdk.Metrics.Base;
using Snd.Sdk.Storage.Base;

namespace Arcane.Stream.RestApi.Extensions;

public static class RestApiSourceExtensions
{
    
    public static IRunnableGraph<(UniqueKillSwitch, Task)> BuildGraph(this RestApiSource source,
        MetricsService metricsService,
        IBlobStorageWriter blobStorageWriter,
        string sinkLocation,
        IStreamContext context,
        int rowsPerGroup,
        TimeSpan groupingInterval)
    {
        var dimensions = source.GetDefaultTags().GetAsDictionary();
        dimensions["mode"] = context.IsBackfilling ? "backfill" : "stream";
        dimensions["streamId"] = context.StreamId;
        dimensions["streamKind"] = context.StreamKind;
        var jsonSink = context.MultilineJsonSinkFromContext(source.GetParquetSchema(), blobStorageWriter, sinkLocation);
        return Source.FromGraph(source)
            .GroupedWithin(rowsPerGroup, groupingInterval)
            .Select(grp =>
            {
                var rows = grp.ToList();
                metricsService.Increment(DeclaredMetrics.ROWS_INCOMING, dimensions, rows.Count);
                return rows;
            })
            .Log(context.StreamKind)
            .ViaMaterialized(KillSwitches.Single<List<JsonElement>>(), Keep.Right)
            .ToMaterialized(jsonSink, Keep.Both);
    }
    

    public static MultilineJsonSink MultilineJsonSinkFromContext(this IStreamContext streamContext, Schema schema, IBlobStorageWriter blobStorageWriter, string sinkLocation)
    {
        var jsonSink = MultilineJsonSink.Create(
            blobStorageWriter,
            $"{sinkLocation}/{streamContext.StreamId}",
            schema,
            streamContext.IsBackfilling ? "backfill" : "data",
            dropCompletionToken: streamContext.IsBackfilling);
        return jsonSink;
    }
}
