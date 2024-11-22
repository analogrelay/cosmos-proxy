using System.Net;
using Grpc.Core;
using Microsoft.Azure.Cosmos.Proxy.Protos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Cosmos.Proxy.Services;

internal class ContainerService : Protos.Container.ContainerBase
{
    private readonly ILogger<ContainerService> _logger;
    private readonly IOptions<ProxyOptions> _proxyOptions;

    public ContainerService(ILogger<ContainerService> logger, IOptions<ProxyOptions> proxyOptions)
    {
        _logger = logger;
        _proxyOptions = proxyOptions;
    }

    public override async Task<QueryReply> QueryItems(QueryRequest request, ServerCallContext context)
    {
        if(!_proxyOptions.Value.Accounts.TryGetValue(request.Container.AccountMoniker, out var accountOptions))
        {
            return new QueryReply()
            {
                Error = new Error()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Account '{request.Container.AccountMoniker}' not found.",
                }
            };
        }

        // TODO: Caching clients! This is just a simple example.
        using var client = accountOptions.CreateCosmosClient();
        var container = client.GetContainer(request.Container.DatabaseId, request.Container.ContainerId);
        var iterator = container.GetItemQueryIterator<JObject>(
            request.Query.Text,
            string.IsNullOrEmpty(request.ContinuationToken) ? null : request.ContinuationToken);

        var page = new DocumentPage();
        if(iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            if (response.ContinuationToken is not null)
            {
                page.ContinuationToken = response.ContinuationToken;
            }

            foreach(var item in response)
            {
                page.Documents.Add(new Document()
                {
                    Json = item.ToString()
                });
            }
        }
        return new QueryReply()
        {
            Success = page,
        };
    }
}
