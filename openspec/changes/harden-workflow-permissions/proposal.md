## Why

OpenSSF Scorecard reports three high-severity Token-Permissions findings because trusted release automation grants broad workflow-level write access or retains a necessary release write permission without an explicit residual-risk record. The OpenSSF Best Practices project is still `in_progress` despite meeting the substantive Passing requirements, because its prepared evidence has not been submitted and externally verified.

## What Changes

- Restrict `GITHUB_TOKEN` permissions to the specific jobs that need release, tag, changelog, package, identity-token, or pull-request write access.
- Preserve the main-branch publishing job's required `contents: write` permission for GitHub release creation, while documenting its trusted-trigger boundary, justification, owner, and exit criteria as a residual Scorecard signal.
- Establish repository-level read-only default workflow permissions as defense in depth.
- Submit the existing truthful Best Practices evidence to project 13670, verify the actual resulting badge status, and record the external result without fabricating a grade.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `repository-security-governance`: Define least-privilege workflow permission boundaries, handling of justified Scorecard token-permission findings, and externally verified Best Practices badge progress.

## Impact

- Affected GitHub Actions workflows: `.github/workflows/prerelease.yml`, `.github/workflows/sync-release-notes.yml`, and `.github/workflows/build-and-test.yml`.
- Affected repository configuration: GitHub Actions default workflow permissions.
- Affected assurance evidence: `docs/openssf-assurance.md` and the Best Practices project at `bestpractices.dev/projects/13670`.
- No public application API, package behavior, or new runtime dependency changes.
