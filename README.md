# Cosmos Proxy Prototype

A Prototype Cosmos Proxy Sidecar, written in .NET.

## Building the proxy

### With Docker

* Install Docker
* Run `docker build -t cosmos-proxy:local .`

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
docker run -e Proxy__Accounts__Default__Endpoint -e Proxy__Accounts__Default__AccountKey -p 5050:5050 -p 5051:5051 cosmos-proxy:local
```

### With .NET

* Run `dotnet run --project src/cosmos-proxy/cosmos-proxy.csproj`, if the environment variables are set they will be read automatically.

## Using the proxy

### With GRPCui

* Install [GRPCui](https://github.com/fullstorydev/grpcui)
* Run `grpcui -plaintext localhost:5051`

### With Go Sample

* Install [Go](https://golang.org/)
* Cd into `samples/go`
* Run `go run . localhost:5051 <cosmos-database> <cosmos-container> <query>`
    For example, `go run . localhost:5051 mydatabase mycontainer "SELECT * FROM c"`