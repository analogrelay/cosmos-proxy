using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Proxy.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("/")]
[Route("/accounts/{accountMoniker}")]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IOptions<ProxyOptions> _proxyOptions;

    public AccountController(ILogger<AccountController> logger, IOptions<ProxyOptions> proxyOptions)
    {
        _logger = logger;
        _proxyOptions = proxyOptions;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccountPropertiesAsync(string? accountMoniker)
    {
        var actualMoniker = accountMoniker ?? "Default";
        if (!_proxyOptions.Value.Accounts.TryGetValue(actualMoniker, out var accountOptions))
        {
            return Problem(
                $"No account named {accountMoniker} has been configured",
                statusCode: 404);
        }

        _logger.LogInformation("Forwarding account properties request for account {AccountMoniker} to {AccountEndpoint}", actualMoniker, accountOptions.Endpoint);

        var client = accountOptions.CreateCosmosClient();
        var properties = await client.ReadAccountAsync();
        var endpoint = Request.Scheme + "://" + Request.Host;
        if (!string.IsNullOrEmpty(accountMoniker))
        {
            endpoint += "/accounts/" + accountMoniker;
        }
        return Json(new
        {
            writeableLocations = new object[] {
                new {
                    name = "Local",
                    databaseAccountEndpoint = endpoint,
                },
            },
            readableLocations = new object[] {
                new {
                    name = "Local",
                    databaseAccountEndpoint = endpoint,
                },
            },
            enableMultipleWriteLocations = false,
            userConsistencyPolicy = properties.Consistency,
        });
    }
}
