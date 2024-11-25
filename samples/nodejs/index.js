const { CosmosClient } = require("@azure/cosmos");
const { createDefaultHttpClient } = require("@azure/core-rest-pipeline");

console.log(process.argv);
if (process.argv.length < 6) {
    console.log("Usage: node index.js <host>:<port> <db> <container> <query>");
}

const target = process.argv[2];
const database = process.argv[3];
const container = process.argv[4];
const query = process.argv[5];

async function main() {
    const httpClient = createDefaultHttpClient();
    const client = new CosmosClient({
        endpoint: `https://${target}`,
        key: "irrelevant",
    });
    const containerClient = client.database(database).container(container);
    const iterator = await containerClient.items.query(query).fetchAll();
    for (const doc of iterator.resources) {
        console.log("Item:");
        console.log(`  ${JSON.stringify(doc)}`);
    }
}

main().catch((error) => {
    console.error(error);
    process.exit(1);
}).then(() => { process.exit(0); });
