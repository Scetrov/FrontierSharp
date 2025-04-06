# syntax=docker/dockerfile:1.4
ARG BASE_IMAGE=mcr.microsoft.com/dotnet/runtime-deps:9.0

FROM ${BASE_IMAGE} AS base
WORKDIR /app

FROM base AS all
COPY ./bin/Release/net9.0/linux-arm64 ./linux-arm64
COPY ./bin/Release/net9.0/linux-x64 ./linux-amd64

FROM base AS final
ARG TARGETARCH

COPY --from=all /app/linux-${TARGETARCH}/publish/ .

ENTRYPOINT ["/app/frontierctl"]