package main

import (
	"context"
	"fmt"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
	"github.com/analogrelay/cosmos-proxy/samples/go/proto"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {
	// The first argument is the target host/port
	if len(os.Args) < 5 {
		fmt.Printf("Usage: %s <endpoint> <db> <container> <query>\n", os.Args[0])
		return
	}

	target := os.Args[1]
	db := os.Args[2]
	container := os.Args[3]
	query := os.Args[4]

	//ProxyViaGrpc(target, db, container, query)
	ProxyViaRestSdk(target, db, container, query)
}

func ProxyViaRestSdk(target, db, container, query string) {
	client, err := azcosmos.NewClientFromConnectionString("AccountEndpoint="+target+";AccountKey=yeep;", nil)
	if err != nil {
		fmt.Printf("Failed to create client: %v\n", err)
		return
	}
	containerClient, err := client.NewContainer(db, container)
	if err != nil {
		fmt.Printf("Failed to get container: %v\n", err)
		return
	}
	queryPager := containerClient.NewQueryItemsPager(
		query,
		azcosmos.NewPartitionKeyString("IGNORED"),
		nil,
	)

	for queryPager.More() {
		queryResponse, err := queryPager.NextPage(context.TODO())
		if err != nil {
			fmt.Printf("Failed to query items: %v\n", err)
			return
		}

		for _, item := range queryResponse.Items {
			itemStr := string(item)
			fmt.Println("Item:")
			fmt.Printf("  %s\n", itemStr)
		}
	}
}

func ProxyViaGrpc(target, db, container, query string) {
	conn, err := grpc.NewClient(target,
		grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		fmt.Printf("Failed to connect to %s: %v\n", target, err)
		return
	}
	defer conn.Close()

	client := proto.NewContainerClient(conn)

	req := proto.QueryRequest{
		Container: &proto.ContainerReference{
			AccountMoniker: "Default",
			DatabaseId:     db,
			ContainerId:    container,
		},
		Query: &proto.Query{
			Text: query,
		},
	}
	reply, err := client.QueryItems(context.TODO(), &req)
	if err != nil {
		fmt.Printf("Failed to query items: %v\n", err)
		return
	}

	if reply_err := reply.GetError(); reply_err != nil {
		fmt.Printf("Query failed: %v\n", reply_err.Message)
		return
	}

	for _, item := range reply.GetSuccess().Documents {
		fmt.Println("Item:")
		fmt.Printf("  %s\n", item.GetJson())
	}
}
