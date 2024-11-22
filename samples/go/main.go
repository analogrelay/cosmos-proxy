package main

import (
	"context"
	"fmt"
	"os"

	"github.com/analogrelay/cosmos-proxy/samples/go/proto"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {
	// The first argument is the target host/port
	if len(os.Args) < 6 {
		fmt.Printf("Usage: %s <host>:<port> <account> <db> <container> <query>\n", os.Args[0])
		return
	}

	target := os.Args[1]
	account := os.Args[2]
	db := os.Args[3]
	container := os.Args[4]
	query := os.Args[5]

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
			AccountMoniker: account,
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
