
using Azure.Identity;
using Microsoft.Azure.Cosmos;

public class ProxyOptions {
    public Dictionary<string, CosmosAccountOptions> Accounts { get; set; } = new();
}

public class CosmosAccountOptions
{
    public string? Endpoint { get; set; }
    public string? AccountKey { get; set; }

    internal CosmosClient CreateCosmosClient()
    {
        if (string.IsNullOrEmpty(Endpoint))
        {
            throw new InvalidOperationException("Endpoint is required.");
        }

        if (string.IsNullOrEmpty(AccountKey))
        {
            return new CosmosClient(Endpoint, new DefaultAzureCredential());
        }
        else 
        {
            return new CosmosClient(Endpoint, AccountKey);
        }
    }
}