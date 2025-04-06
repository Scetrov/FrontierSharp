# syntax=docker/dockerfile:1.4
ARG BASE_IMAGE=mcr.microsoft.com/dotnet/runtime-deps:9.0

FROM ${BASE_IMAGE} AS base
WORKDIR /app

FROM base AS build-amd64
COPY ./bin/Release/net9.0/linux-x64/publish/ .

FROM base AS build-arm64
COPY ./bin/Release/net9.0/linux-arm64/publish/ .

FROM base AS final
ARG TARGETARCH

# Use conditional copy based on TARGETARCH
COPY --from=build-${TARGETARCH} /app /app

ENTRYPOINT ["/app/frontierctl"]