using Microsoft.Azure.Cosmos.Proxy.Protos;
using Newtonsoft.Json;

namespace Microsoft.Azure.Cosmos.Proxy.Models;
public class Query
{
    [JsonProperty("query")]
    public required string Text { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<QueryParameter> Parameters { get; set; } = new();
}

public class QueryParameter
{
    public required string Name { get; set; }
    public required object Value { get; set; }
}
