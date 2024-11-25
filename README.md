# Cosmos Proxy Prototype

A Prototype Cosmos Proxy Sidecar, written in .NET.

Functionality is **extremely limited**. Currently, it only supports enough API to support a simple (cross-partition!) query.
Specifically, the following APIs are _partially_ implemented

* "Get Account" API, `GET /`, to fetch account metadata for Global Endpoint Manager.
    * We proxy this request, but provide only a single read/write endpoint, which is this proxy service.
* "Query Documents" API, `POST /dbs/{databaseId}/colls/{collectionId}/docs` with either `x-ms-documentdb-isquery: True` or `x-ms-documentdb-query: True` (some SDKs use one, some use the other...)
    * We run this request through the .NET SDK and return the results.

## Building the proxy

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

For example:

```bash
export Proxy__Accounts__Default__Endpoint=https://myaccount.documents.azure.com:443/
export Proxy__Accounts__Default__AccountKey=<put your secret key here>
```

### With Docker

```
docker run -d -e Proxy__Accounts__Default__Endpoint -e Proxy__Accounts__Default__AccountKey --name cosmos-proxy -p 5051:5051 ghcr.io/analogrelay/cosmos-proxy:local
```

**NOTE:** The docker image currently only supports HTTP on port 5051. If you need HTTPS, you will need to run the .NET app directly.

### With .NET

* Run `dotnet run --project src/cosmos-proxy/cosmos-proxy.csproj`, if the environment variables are set they will be read automatically.

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