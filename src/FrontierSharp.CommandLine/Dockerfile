# syntax=docker/dockerfile:1.4
ARG BASE_IMAGE=mcr.microsoft.com/dotnet/runtime-deps:10.0@sha256:894098eafc82e5fa02ba9f2b71d426dc78252876b9e914caae77ed95cfce185a

FROM ${BASE_IMAGE}
ARG TARGETARCH
WORKDIR /app

COPY ./bin/Release/net10.0/ /tmp/publish/
RUN case "$TARGETARCH" in \
      amd64) cp -a /tmp/publish/linux-x64/publish/. . ;; \
      arm64) cp -a /tmp/publish/linux-arm64/publish/. . ;; \
      *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac \
    && rm -rf /tmp/publish

ENTRYPOINT ["/app/frontierctl"]
