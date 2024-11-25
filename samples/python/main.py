import sys
import json
from azure.cosmos import CosmosClient

if len(sys.argv) < 5:
    print("Usage: python main.py <host>:<port> <database> <container> <query>")
    sys.exit(1)

target = sys.argv[1]
database = sys.argv[2]
container = sys.argv[3]
query = sys.argv[4]

# credential is just 'irrelevant' base64 encoded, the python SDK requires that the value actually be base64 decoded.
client = CosmosClient("https://" + target, credential="aXJyZWxldmFudA==")
pager = client.get_database_client(database).get_container_client(container).query_items(query=query)

for item in pager:
    print("Item: ")
    print(json.dumps(item, indent=True))