# TDD: Dapr

Full cycle example of pubsub with aspnetcore, dapr and rabbitmq.

## Dapr

Install Dapr using the [getting started](https://docs.dapr.io/getting-started/).

Dapr documentation on RabbitMQ as [pubsub component](https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-rabbitmq/)

## RabbitMQ

RabbitMQ with management plugin:

```shell
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 -p 5672:5672 rabbitmq:management-alpine
```
key|value
---|---
url | http://localhost:8080
username | guest
password | guest

## Dapr sidecar

Prior to running the API integration tests you start the dapr runtime process in self-hosted mode:
```
dapr run --app-id test --app-port 5555  --dapr-grpc-port 5000 --components-path  ./DaprDemo.Api.IntegrationTests/components
```

Afterwards you stop it with the terminate signal, or from another process:
```
dapr stop --app-id test
```

[Source](https://docs.dapr.io/developing-applications/sdks/dotnet/dotnet-development/dotnet-development-dapr-cli/#using-the-dapr-cli).
