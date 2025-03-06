# shopFlow.CLI
This is a simple .NET Core console application to handle backoffice and admin tasks.
The only action available at the moment is "MIGRATE" that allow to migrate data from Json files to LiteDB

Usage example:
```shell
[from shopFlow.NET folder]
docker build -t algiro/shop-flow-cli:latest -f shopFlow.CLI/Dockerfile .
docker run -v "C:\data\shopFlowMovs:/data/shopFlowMovs" algiro/shop-flow-cli MIGRATE