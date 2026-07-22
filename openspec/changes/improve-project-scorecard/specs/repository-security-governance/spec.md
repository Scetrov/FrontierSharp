## MODIFIED Requirements

### Requirement: Best Practices evidence is truthful
Any OpenSSF Best Practices link or badge published by the repository SHALL resolve to registered FrontierSharp project 13670 and display its actual current status. The repository SHALL maintain a valid root `.bestpractices.json` containing every currently answerable Passing, Silver, and Gold criterion with truthful project evidence, while unsupported or unknown claims MUST remain unanswered or explicitly unknown.

#### Scenario: Project is not yet registered
- **WHEN** no public Best Practices project record exists for the repository
- **THEN** the repository does not publish a fabricated or preselected status badge

#### Scenario: Registration is completed
- **WHEN** the repository's Best Practices project is publicly available
- **THEN** the README badge image and target resolve to project 13670 and reflect the status returned by the service

#### Scenario: Repository evidence is proposed to Best Practices automation
- **WHEN** bestpractices.dev reads the root `.bestpractices.json`
- **THEN** the file uses supported criterion keys and statuses and provides repository-backed answers across Passing, Silver, and Gold without converting unknown criteria into claims

#### Scenario: Criterion evidence is unavailable
- **WHEN** a Passing, Silver, or Gold criterion cannot be demonstrated by current project or repository evidence
- **THEN** that criterion is omitted or marked unknown and the badge continues to report only the status awarded by bestpractices.dev

## ADDED Requirements

### Requirement: OpenSSF signal verification is reproducible
The repository SHALL record enough non-sensitive evidence to reproduce verification of the Scorecard fuzzing signal and Best Practices automation input, including source revisions, service targets, validation dates, and residual limitations.

#### Scenario: Assurance metadata changes
- **WHEN** the recognized fuzzing integration, `.bestpractices.json`, or README Best Practices badge is added or materially changed
- **THEN** maintainers validate the local configuration and record the corresponding external service result or a clearly owned pending-verification state

#### Scenario: External service has not refreshed
- **WHEN** GitHub, Scorecard, or bestpractices.dev has not yet processed a merged assurance change
- **THEN** the repository records the pending state and does not fabricate a successful score, badge level, or criterion result
