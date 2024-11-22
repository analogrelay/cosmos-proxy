$RepoRoot = Split-Path -Parent $PSScriptRoot

$ProtosDir = Join-Path $RepoRoot "protos"
$GoOutDir = Join-Path (Join-Path $RepoRoot "samples") "go"

protoc --go_out=$GoOutDir --go_opt=module="github.com/analogrelay/cosmos-proxy/samples/go" --go-grpc_out=$GoOutDir --go-grpc_opt=module="github.com/analogrelay/cosmos-proxy/samples/go" --proto_path=$ProtosDir $ProtosDir/*.proto