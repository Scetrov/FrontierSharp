# repository-security-governance Specification

## Purpose
TBD - created by archiving change harden-scorecard-assurance-controls. Update Purpose after archive.
## Requirements
### Requirement: Solo-maintainer branch controls remain enforceable
The default-branch ruleset SHALL enforce the strongest controls that do not require a nonexistent independent reviewer, including pull-request changes, required current CI and dependency-review checks, code scanning, signed commits, linear history, resolved review threads, stale-review dismissal, deletion and force-push prevention, and no bypass actors.

#### Scenario: Pull request has stale or failing required checks
- **WHEN** the pull request branch is behind the required base state or a required build, dependency-review, or code-scanning check fails
- **THEN** GitHub prevents the pull request from merging

#### Scenario: Default branch is changed outside the pull-request path
- **WHEN** an actor attempts a direct, unsigned, non-fast-forward, deletion, or bypassed update prohibited by the ruleset
- **THEN** GitHub rejects the update

### Requirement: Unavailable independent review is not misrepresented
While the repository has only one trusted maintainer, it MUST NOT claim assurance through unusable CODEOWNERS mappings, fabricated identities, self-approval, or artificial review history. The accepted independent-review risk SHALL identify onboarding a trusted second reviewer as its exit criterion.

#### Scenario: Ruleset is reviewed under solo maintainership
- **WHEN** no independent trusted reviewer is available
- **THEN** required approval, last-push approval, and code-owner approval settings remain disabled or are explicitly recorded as unavailable rather than deadlocking maintainer pull requests

#### Scenario: A trusted second reviewer becomes available
- **WHEN** the maintainer approves onboarding an independent reviewer in a future governance change
- **THEN** the residual record directs that change to add valid ownership mappings and real approval requirements before claiming the finding resolved

### Requirement: Vulnerability reporting is direct and coordinated
The root security policy SHALL provide a working direct link to GitHub private vulnerability reporting and SHALL state maintainer-approved numeric targets for acknowledgement, status updates, and coordinated disclosure.

#### Scenario: Reporter follows the security policy
- **WHEN** a reporter opens the private-reporting link from `SECURITY.md`
- **THEN** the reporter reaches the repository's private vulnerability-reporting flow without being directed to a public issue

#### Scenario: Maintainer coordinates a report
- **WHEN** a private vulnerability report is submitted
- **THEN** the policy provides concrete acknowledgement and update targets and a disclosure expectation that permits coordinated remediation

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

### Requirement: Scorecard remediation evidence is auditable
The change SHALL record the Scorecard version and run date, map every original alert number to validation evidence, and classify any remaining alert as external, temporal, accepted solo-maintainer risk, or reproduced scanner limitation with an owner and exit criterion.

#### Scenario: Post-change Scorecard run completes
- **WHEN** all implementable controls have been installed and their focused validation passes
- **THEN** a fresh Scorecard run records the outcome for alerts #2 through #12 and links each closed alert to its control evidence

#### Scenario: History-derived code-review alert remains
- **WHEN** Scorecard continues to report unreviewed recent changesets under the solo-maintainer model
- **THEN** the alert remains documented as an accepted temporal governance risk whose exit requires a real second reviewer and future independently approved changesets

#### Scenario: Docker alias alert remains after structural remediation
- **WHEN** the Dockerfile contains no mutable external image or ambiguous local-stage `FROM` instruction but Scorecard still reports one
- **THEN** maintainers preserve the scan and Docker build evidence before considering a scanner-false-positive dismissal

### Requirement: Security controls are not weakened for score optimization
No remediation SHALL disable an existing security check, loosen workflow permissions, expose credentials, or add nonfunctional evidence solely to improve the Scorecard result.

#### Scenario: Proposed metric fix reduces actual assurance
- **WHEN** a proposed change would improve a Scorecard score by bypassing validation, fabricating evidence, or weakening an existing control
- **THEN** the proposal is rejected and the residual finding is documented instead

### Requirement: OpenSSF signal verification is reproducible
The repository SHALL record enough non-sensitive evidence to reproduce verification of the Scorecard fuzzing signal and Best Practices automation input, including source revisions, service targets, validation dates, and residual limitations.

#### Scenario: Assurance metadata changes
- **WHEN** the recognized fuzzing integration, `.bestpractices.json`, or README Best Practices badge is added or materially changed
- **THEN** maintainers validate the local configuration and record the corresponding external service result or a clearly owned pending-verification state

#### Scenario: External service has not refreshed
- **WHEN** GitHub, Scorecard, or bestpractices.dev has not yet processed a merged assurance change
- **THEN** the repository records the pending state and does not fabricate a successful score, badge level, or criterion result

