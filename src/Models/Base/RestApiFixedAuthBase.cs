using System.Collections.Generic;

namespace Arcane.Stream.RestApi.Models.Base;

public class RestApiFixedAuthBase : RestApiStreamContextBase
{
    /// <summary>
    /// Auth information provided in custom/default headers in a form header: value
    /// </summary>
    public Dictionary<string, string> AuthHeaders { get; private set; }

    public override RestApiStreamContextBase LoadSecrets()
    {
        this.AuthHeaders = this.GetJSONSecretFromEnvironment<Dictionary<string, string>>(nameof(this.AuthHeaders));
        return this;
    }
}
