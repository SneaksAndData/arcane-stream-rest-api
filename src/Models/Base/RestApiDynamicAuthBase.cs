using System;

namespace Arcane.Stream.RestApi.Models.Base;

public class RestApiDynamicAuthBase : RestApiStreamContextBase
{
    /// <summary>
    /// Url to retrieve access tokens from.
    /// </summary>
    public string AuthUrl { get; private set; }

    /// <summary>
    /// Name of the response property that contains access token value.
    /// </summary>
    public string TokenPropertyName { get; private set; }

    /// <summary>
    /// Name of the response property that contains access token expiration (duration) period.
    /// </summary>
    public string ExpirationPeriodPropertyName { get; private set; }
    
    /// <summary>
    /// Optional fixed expiration period for the token.
    /// </summary>
    public TimeSpan? ExpirationPeriod { get; init; }

    /// <summary>
    /// Optional body that contains information required to issue a token.
    /// </summary>
    public string TokenRequestBody { get; private set; }

    /// <summary>
    /// Optional content type for the body. Defaults to application/json if not provided.
    /// </summary>
    public string TokenRequestContentType { get; private set; }

    /// <summary>
    /// Http method to be used with token issue endpoint: GET or POST.
    /// </summary>
    public string TokenHttpMethod { get; private set; }

    /// <summary>
    /// Additional headers (content-type, accept etc. for token request, json serialized).
    /// </summary>
    public string TokenRequestAdditionalHeaders { get; private set; }

    /// <summary>
    /// Header to supply the token to. Defaults to `Authorization` if not provided.
    /// </summary>    
    public string TokenRequestHeaderName { get; init; }

    /// <summary>
    /// Token scheme to provide in the header before the token value. Defaults to `Bearer` if not provided and TokenRequestHeaderName is `Authorization`.
    /// If not provided and `TokenRequestHeaderName` is other value than "Authorization", defaults to empty string. 
    /// </summary>    
    public string TokenRequestTokenScheme { get; private set; }

    public override RestApiStreamContextBase LoadSecrets()
    {
        this.AuthUrl = this.GetSecretFromEnvironment(nameof(this.AuthUrl));
        this.ExpirationPeriodPropertyName = this.GetSecretFromEnvironment(nameof(this.ExpirationPeriodPropertyName));
        this.TokenHttpMethod = this.GetSecretFromEnvironment(nameof(this.TokenHttpMethod));
        this.TokenPropertyName =  this.GetSecretFromEnvironment(nameof(this.TokenPropertyName));
        this.TokenRequestAdditionalHeaders = this.GetSecretFromEnvironment(nameof(this.TokenRequestAdditionalHeaders));
        this.TokenRequestBody = this.GetSecretFromEnvironment(nameof(this.TokenRequestBody));
        this.TokenRequestContentType = this.GetSecretFromEnvironment(nameof(this.TokenRequestContentType));
        this.TokenRequestTokenScheme = this.GetSecretFromEnvironment(nameof(this.TokenRequestTokenScheme));
        return this;
    }
}
