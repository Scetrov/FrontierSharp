# parser-robustness-assurance Specification

## Purpose

Define the minimum parser fuzzing assurance for the FrontierSharp repository,
including the coverage-guided SharpFuzz/AFL++ workflow and the documented
OpenSSF Scorecard detection limitation.

## Requirements

### Requirement: Operational parser fuzzing is coverage-guided and bounded
The repository SHALL run its existing SharpFuzz/AFL++ parser targets as scheduled
or manually dispatched, coverage-guided campaigns. Each target MUST use committed
non-sensitive corpus inputs, have an explicit execution bound, run with read-only
repository access, and have no repository secrets or publishing permissions.

#### Scenario: Scheduled or manual fuzz campaign executes
- **WHEN** a scheduled or manually dispatched fuzz campaign runs
- **THEN** each FrontierSharp parser target executes independently with a bounded
  AFL++ campaign and the workflow preserves the deterministic pull-request corpus
  replay gate

#### Scenario: Campaign preserves operational evidence
- **WHEN** a campaign completes or reports an unexpected finding
- **THEN** the workflow emits target-scoped logs and preserves sanitized,
  bounded-retention crash or hang artifacts that can be replayed using the
  existing harness and committed corpus layout

### Requirement: Scorecard false negatives do not weaken fuzzing controls
The repository SHALL document the current OpenSSF Scorecard Fuzzing result and its
detection limitation without representing that limitation as an absence of
operational fuzzing.

#### Scenario: Scorecard does not recognize SharpFuzz
- **WHEN** a current Scorecard analysis assigns a Fuzzing score of `0` because it
  does not recognize SharpFuzz/AFL++
- **THEN** maintainers retain the functioning AFL++ integration, record the
  Scorecard version and finding, and do not add a decorative ClusterFuzzLite or
  OSS-Fuzz configuration solely to affect detection

#### Scenario: Scorecard support changes
- **WHEN** a released Scorecard version adds a detector that could recognize an
  additional FrontierSharp test integration
- **THEN** maintainers evaluate it only when it provides useful test coverage and
  is operationally supported; detector recognition is not by itself an acceptance
  criterion
