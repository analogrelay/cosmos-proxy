using Microsoft.Azure.Cosmos.Proxy.Protos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DocumentPageResponse
{
    [JsonProperty("Documents")]
    public required List<JObject> Documents { get; set; }
}
