# syntax=docker/dockerfile:1.4

# Define base image for multi-platform
ARG BASE_IMAGE=mcr.microsoft.com/dotnet/runtime-deps:9.0

# Use platform-specific build stages
FROM ${BASE_IMAGE} AS base
WORKDIR /app

# Copy the correct publish output based on the platform
FROM base AS x64
COPY ./bin/Release/net9.0/linux-x64/publish/ .

FROM base AS arm64
COPY ./bin/Release/net9.0/linux-arm64/publish/ .

# Use target platform to decide what files to copy
FROM base AS final
ARG TARGETARCH
COPY --from=${TARGETARCH} /app /app
ENTRYPOINT ["/app/FrontierSharp.CommandLine"]
