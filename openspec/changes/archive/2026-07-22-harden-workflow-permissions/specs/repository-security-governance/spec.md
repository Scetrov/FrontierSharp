## ADDED Requirements

### Requirement: Automation write authority is least-privilege
GitHub Actions workflows SHALL use a read-only workflow-level permission baseline. A job SHALL declare a write permission only when that job performs the corresponding repository, release, package, identity-token, or pull-request operation, and the declaration SHALL contain no unrelated write scopes.

#### Scenario: A workflow includes both read-only and release operations
- **WHEN** a workflow contains a job that reads source code and a different job that creates a release, tag, changelog commit, package, or pull-request label
- **THEN** only the job that performs each write operation receives its corresponding write permission

#### Scenario: A new job is added to an affected workflow
- **WHEN** a maintainer adds a job without an explicit permissions declaration
- **THEN** the job receives only the workflow's read-only baseline and no inherited write authority

### Requirement: Necessary publishing authority is an auditable residual risk
When trusted main-branch publishing requires `contents: write` to create a GitHub release, the repository SHALL keep that authority isolated to the publishing job and SHALL record its alert identifier, purpose, trigger boundary, owner, validation evidence, and exit criterion in assurance evidence. The record MUST NOT characterize the alert as resolved unless the permission is removed or a fresh scanner result confirms resolution.

#### Scenario: Main publishing creates a GitHub release
- **WHEN** the trusted publishing job creates the project's GitHub release after a successful main-branch build
- **THEN** it receives `contents: write` only at that job boundary and retains only the other write scopes required for its existing publication operations

#### Scenario: Scorecard continues to flag the publishing permission
- **WHEN** a fresh Scorecard analysis reports the Token-Permissions alert for the scoped publishing job
- **THEN** assurance evidence identifies it as a justified residual signal with an owner and exit criterion rather than falsely claiming it is fixed

### Requirement: Best Practices badge submission is externally verified
The maintainer SHALL submit truthful repository evidence through the supported Best Practices flow for project 13670 and SHALL record the resulting service timestamp, badge level, and any unmet required criteria. The repository MUST NOT claim a Passing badge until bestpractices.dev reports it.

#### Scenario: Best Practices awards Passing
- **WHEN** bestpractices.dev reports a Passing badge for project 13670 after evidence submission
- **THEN** assurance evidence records the reported timestamp and badge level, and the README badge continues to resolve to that project

#### Scenario: Best Practices remains in progress
- **WHEN** bestpractices.dev does not award Passing after evidence submission
- **THEN** assurance evidence records the actual `in_progress` result and the outstanding required criteria without modifying repository claims to imply a Passing grade
