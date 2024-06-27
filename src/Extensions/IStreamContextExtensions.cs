using System;
using Arcane.Framework.Services.Base;
using Arcane.Stream.RestApi.Models;

namespace Arcane.Stream.RestApi.Extensions;

public static class IStreamContextExtensions
{
    public static string GetSinkLocation(this IStreamContext streamContext)
    {
        return streamContext switch
        {
            RestApiDynamicAuthStreamContext restApiPagedFixedAuthStreamContext => restApiPagedFixedAuthStreamContext.SinkLocation,
            RestApiFixedAuthStreamContext restApiFixedAuthStreamContext => restApiFixedAuthStreamContext.SinkLocation,
            RestApiPagedDynamicAuthStreamContext restApiPagedDynamicAuthStreamContext => restApiPagedDynamicAuthStreamContext.SinkLocation,
            RestApiPagedFixedAuthStreamContext restApiPagedFixedAuthStreamContext => restApiPagedFixedAuthStreamContext.SinkLocation,
            _ => throw new InvalidOperationException($"Unknown stream context type {streamContext.GetType()}")
        };
    }
}
