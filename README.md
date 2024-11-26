# Cosmos Proxy Prototype

A Prototype Cosmos Proxy Sidecar, written in .NET.

Functionality is **extremely limited**. Currently, it only supports enough API to support a simple (cross-partition!) query.
Specifically, the following APIs are _partially_ implemented

* "Get Account" API, `GET /`, to fetch account metadata for Global Endpoint Manager.
    * We proxy this request, but provide only a single read/write endpoint, which is this proxy service.
* "Query Documents" API, `POST /dbs/{databaseId}/colls/{collectionId}/docs` with either `x-ms-documentdb-isquery: True` or `x-ms-documentdb-query: True` (some SDKs use one, some use the other...)
    * We run this request through the .NET SDK and return the results.

In addition to the HTTP APIs, a gRPC API is provided.
The gRPC API allows us to explore a protobuf API for the proxy which may provide more flexibility and performance than the HTTP API.
However, the SDK would need to be updated to support this new API.

## Building the proxy

*Building is not necessary to test out the proxy, you can pull the image from the GitHub Container Registry. Skip to "Running the proxy / With Docker".*

### With Docker

* Install Docker
* Run `.\build.ps1`

### With .NET

* Install .NET SDK
* Run `dotnet build`

## Running the proxy

To define a proxy account, you need to set the following environment variables:

* `Proxy__Accounts__[name]__Endpoint` - The Cosmos DB endpoint, such as `https://myaccount.documents.azure.com:443/`
* `Proxy__Accounts__[name]__AccountKey` - The Cosmos DB account key (or, omit this to use Entra ID)

The `[name]` placeholder is a unique name within the proxy to identify the account.
The value `Default` is reserved for the default account, which is used when no account is specified.
Otherwise, it can be whatever you'd like.

### With Docker

```
docker run -d -e DOTNET_ENVIRONMENT=Development -e Proxy__Accounts__Default__Endpoint=<endpoint_url> -e Proxy__Accounts__Default__AccountKey=<account_key> --name cosmos-proxy -p 5051:5051 ghcr.io/analogrelay/cosmos-proxy:local
```

**NOTE:** This configures the proxy to run in "Development" mode, which includes Swagger UI and other utilities. In production, you should omit the `-e DOTNET_ENVIRONMENT=Development` flag.

Monitor logs with:

```
docker logs -f cosmos-proxy
```

(Press "Ctrl-C" to stop monitoring logs. This will NOT stop the proxy.)

Stop the proxy with:

```
docker stop cosmos-proxy
```

Remove the proxy container with:

```
docker rm cosmos-proxy
```

**NOTE:** The docker image currently only supports HTTP on port 5051. If you need HTTPS, you will need to run the .NET app directly.

Verify the proxy is up by navigating to `http://localhost:5051/`. You should see something like this:

```json
{
  "writeableLocations": [
    {
      "name": "Local",
      "databaseAccountEndpoint": "http://localhost:5051"
    }
  ],
  "readableLocations": [
    {
      "name": "Local",
      "databaseAccountEndpoint": "http://localhost:5051"
    }
  ],
  "enableMultipleWriteLocations": false,
  "userConsistencyPolicy": {
    "defaultConsistencyLevel": "Session",
    "maxStalenessPrefix": 0,
    "maxIntervalInSeconds": 0
  }
}
```

Now, proceed to "Using the proxy".

### With .NET

First, set the necessary environment variables.

For example:

```bash
export Proxy__Accounts__Default__Endpoint=https://myaccount.documents.azure.com:443/
export Proxy__Accounts__Default__AccountKey=<put your secret key here>
```

Then, run:

```bash
dotnet run --project src/cosmos-proxy/cosmos-proxy.csproj
```

Verify the proxy is up by navigating to `https://localhost:5050/`. You should see something like this:

```json
{
  "writeableLocations": [
    {
      "name": "Local",
      "databaseAccountEndpoint": "http://localhost:5051"
    }
  ],
  "readableLocations": [
    {
      "name": "Local",
      "databaseAccountEndpoint": "http://localhost:5051"
    }
  ],
  "enableMultipleWriteLocations": false,
  "userConsistencyPolicy": {
    "defaultConsistencyLevel": "Session",
    "maxStalenessPrefix": 0,
    "maxIntervalInSeconds": 0
  }
}
```

Now, proceed to "Using the proxy".

## Using the proxy

### With GRPCui

* Install [GRPCui](https://github.com/fullstorydev/grpcui)
* Run `grpcui -plaintext localhost:5051`

### With Go Sample

* Install [Go](https://golang.org/)
* Cd into `samples/go`
* Run `go run . http://localhost:5051 <cosmos-database> <cosmos-container> <query>`

### With NodeJS Sample

* Install Node
* Cd into `samples/nodejs`
* Run `node index.js http://localhost:5051 <cosmos-database> <cosmos-container> <query>`

### With Python Sample

* Install Python
* Cd into `samples/python`
* (Optional) Set up a virtual environment, `python -m venv .venv` and activate it with `source .venv/bin/activate` / `.\.venv\Scripts\Activate.ps1`
* Run `pip install -r requirements.txt`
* Run `python main.py http://localhost:5051 <cosmos-database> <cosmos-container> <query>`