# parser-robustness-assurance Specification

## Purpose
TBD - created by archiving change harden-scorecard-assurance-controls. Update Purpose after archive.
## Requirements
### Requirement: Coverage-guided parser harnesses
The repository SHALL provide SharpFuzz-compatible, coverage-guided harnesses for the highest-risk untrusted-input parsing boundaries without requiring production network services or user files.

#### Scenario: Hermetic parser targets
- **WHEN** arbitrary input is supplied to a Sui GraphQL/Move-object JSON, `ResIndex`, Razorvine pickle, or World API response target
- **THEN** the target exercises the production parsing boundary without live network or arbitrary host-path access.

### Requirement: Expected invalid input is distinguished from defects
Each harness SHALL classify documented invalid-input outcomes as rejected inputs while allowing unexpected exceptions, process crashes, hangs, or resource-limit violations to be reported as fuzzing findings.

#### Scenario: Parser rejects malformed input normally
- **WHEN** arbitrary input produces an expected parse or validation failure
- **THEN** the harness completes the iteration without recording a false crash.

### Requirement: Fuzzing is hermetic and bounded
Fuzz harnesses and workflows MUST NOT access live EVE Frontier services, arbitrary host paths, credentials, publishing permissions, or mutable user data, and every fuzz run SHALL have explicit execution bounds.

#### Scenario: Fuzz job executes in CI
- **WHEN** a pull-request smoke run or scheduled fuzz run starts
- **THEN** it runs with minimum permissions, no repository secrets, no publishing dependency, and explicit time limits.

### Requirement: Seed corpora and failures are reproducible
The repository SHALL keep minimized, non-sensitive seed corpora for every fuzz target and SHALL provide deterministic corpus replay in normal CI.

#### Scenario: Deterministic replay in CI
- **WHEN** pull-request validation runs against committed corpora for Sui, ResIndex, Pickle, and World targets
- **THEN** each seed replays deterministically and any unexpected failure blocks the validation job.

### Requirement: Fuzz tooling compatibility is proven and pinned
The SharpFuzz toolchain and package dependencies SHALL be pinned to reviewed versions, and compatibility with the repository's .NET target and Linux runner MUST be demonstrated before the full fuzz workflow is treated as operational.

#### Scenario: Minimal compatibility target runs
- **WHEN** SharpFuzz 2.3.0 instruments a pure-IL target assembly and a framework-dependent .NET 10 harness executes it under AFL++
- **THEN** the run records nonzero coverage and exits within its configured bounds with `DOTNET_ReadyToRun=0`, `DOTNET_TieredCompilation=0`, and `DOTNET_ROLL_FORWARD=Major`.

