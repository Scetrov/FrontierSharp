# WorldApiClient TODOs

A concise list of high-level tasks and the World API endpoints they relate to.

- Implement single-entity retrieval endpoints
  - GET /v2/killmails/{id}  (GetKillmailById)
  - GET /v2/tribes/{id}     (GetTribeById)

- Add POD verification support
  - POST /v2/pod/verify      (VerifyPod)

- Add `format` support for single-resource endpoints (json / pod)
  - affects: /v2/killmails/{id}, /v2/smartassemblies/{id}, /v2/tribes/{id}, /v2/types/{id}, /v2/solarsystems/{id}, /v2/smartcharacters/{address}

- Add filtering options on list endpoints
  - GET /v2/smartassemblies (add `type` query param)

- Implement or surface meta endpoints
  - GET /health

- Tests, fixtures & infra
  - Add unit tests and embedded payloads for each new endpoint/format variant
  - Add request models and update `IWorldApiClient` + `WorldApiClient` implementations



