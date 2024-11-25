using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Proxy.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("/dbs/{databaseId}/colls")]
[Route("/accounts/{accountMoniker}/dbs/{databaseId}/colls")]
public class ContainerController: Controller
{
    private readonly ILogger<ContainerController> _logger;
    private readonly IOptions<ProxyOptions> _proxyOptions;

    public ContainerController(ILogger<ContainerController> logger, IOptions<ProxyOptions> proxyOptions)
    {
        _logger = logger;
        _proxyOptions = proxyOptions;
    }

    [IsQueryRequest]
    [HttpPost("{containerId}/docs")]
    [Consumes("application/json", "application/query+json")]
    public async Task<IActionResult> QueryDocumentsAsync(string? accountMoniker, string databaseId, string containerId, [FromHeader(Name = "x-ms-continuation")] string? continuationToken, [FromBody] Query query)
    {
        accountMoniker ??= "Default";
        if (!_proxyOptions.Value.Accounts.TryGetValue(accountMoniker, out var accountOptions))
        {
            return Problem(
                $"No account named {accountMoniker} has been configured",
                statusCode: 404);
        }

        _logger.LogInformation("Forwarding query for account {AccountMoniker}, database {DatabaseId}, container {ContainerId} to {AccountEndpoint}", accountMoniker, databaseId, containerId, accountOptions.Endpoint);
        _logger.LogInformation("Query: {QueryText}", query.Text);

        // We are ignoring whatever partition key comes from the client, because the proxy can do full query parsing and cross-partition queries (because we're using the .NET SDK).

        // TODO: Caching clients! This is just a simple example.
        using var client = accountOptions.CreateCosmosClient();
        var container = client.GetContainer(databaseId, containerId);
        var iterator = container.GetItemQueryIterator<JObject>(
            query.Text,
            continuationToken);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            if (response.ContinuationToken is not null)
            {
                Response.Headers["x-ms-continuation"] = response.ContinuationToken;
            }
            _logger.LogInformation("Received {DocumentCount} documents from {AccountMoniker}/{DatabaseId}/{ContainerId}", response.Count, accountMoniker, databaseId, containerId);

            return Json(new DocumentPageResponse()
            {
                Documents = response.ToList(),
            });
        }
        _logger.LogInformation("Received no documents from {AccountMoniker}/{DatabaseId}/{ContainerId}", accountMoniker, databaseId, containerId);
        return Json(new DocumentPageResponse()
        {
            Documents = new(),
        });
    }
}
