using System;
using Amazon.S3;
using Arcane.Framework.Services.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snd.Sdk.Storage.Amazon;
using Snd.Sdk.Storage.Base;
using Snd.Sdk.Storage.Models.BlobPath;
using Snd.Sdk.Storage.Providers.Configurations;

namespace Arcane.Stream.RestApi.Extensions;

public static class ServiceProviderExtensions
{
    public static IBlobStorageWriter GetBlobStorageWriter(this IServiceProvider sp)
    {
        var context = sp.GetRequiredService<IStreamContext>();
        var pathType = context.GetSinkLocation();
        
        if (AdlsGen2Path.IsAdlsGen2Path(pathType))
        {
            return sp.GetRequiredService<IBlobStorageService>();
        }

        if (AmazonS3StoragePath.IsAmazonS3Path(pathType))
        {
            var awsConfig = AmazonStorageConfiguration.CreateFromEnv();
            var clientConfig = new AmazonS3Config
            {
                UseHttp = awsConfig.UseHttp,
                ForcePathStyle = true,
                ServiceURL = awsConfig.ServiceUrl.ToString(),
                UseAccelerateEndpoint = false,
            };

            var client = new AmazonS3Client(awsConfig.AccessKey, awsConfig.SecretKey, clientConfig);
            return new AmazonBlobStorageWriter(client, sp.GetRequiredService<ILogger<AmazonBlobStorageWriter>>());
        }

        throw new InvalidOperationException(
            $"Unknown storage path type: {pathType}. Cannot create blob storage writer.");
    }
}
