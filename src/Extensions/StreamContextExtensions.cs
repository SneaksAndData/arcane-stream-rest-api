using System;
using Arcane.Framework.Services.Base;
using Arcane.Stream.RestApi.Models.Base;

namespace Arcane.Stream.RestApi.Extensions;

public static class StreamContextExtensions
{
    public static string GetSinkLocation(this IStreamContext streamContext)
    {
        return streamContext switch
        {
            RestApiStreamContextBase context => context.SinkLocation,
            _ => throw new InvalidOperationException($"Unknown stream context type {streamContext.GetType()}")
        };
    }
}
