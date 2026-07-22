## Context

Three open high-severity OpenSSF Scorecard Token-Permissions alerts originate in release automation. `prerelease.yml` and `sync-release-notes.yml` grant `contents: write` at workflow scope, allowing jobs that only need reads to inherit write authority. `build-and-test.yml` correctly isolates publishing authority to a `publish` job, but that job needs `contents: write` to create the GitHub release, as well as `packages: write` and `id-token: write` for publication.

The repository currently defaults newly created workflow tokens to write access. The Best Practices project (13670) has truthful local evidence and satisfies the substantive Passing requirements, but its public badge remains `in_progress` until a maintainer submits the evidence and the service reports an authoritative outcome.

## Goals / Non-Goals

**Goals:**
- Make read access the workflow baseline and grant write permissions only to the jobs that perform the corresponding operation.
- Preserve release publication behavior and its required trust boundary.
- Make the remaining publish-job Token-Permissions finding auditable rather than treating it as resolved without evidence.
- Obtain and record an authoritative Best Practices service result without claiming a grade before it exists.

**Non-Goals:**
- Remove or replace GitHub release creation, GitHub Packages publication, NuGet trusted publishing, or PR pre-release behavior.
- Chase a Scorecard-perfect result by weakening release controls or introducing a separate credential.
- Claim that Best Practices has awarded Passing before the public project record says so.
- Address unrelated Scorecard findings or application-code security concerns.

## Decisions

### Default to read-only and elevate at the job boundary

Each affected workflow will declare a read-only top-level baseline. Jobs that create releases, tags, changelog commits, packages, OIDC tokens, or pull-request labels will declare only their required write scopes. This limits accidental authority inheritance when workflows gain new jobs and confines powerful tokens to narrow execution paths.

**Alternative considered:** retain top-level write permissions and rely on current job conditions. Rejected because it leaves unrelated jobs with authority they do not need and is the direct cause of alerts #14 and #15.

### Retain `contents: write` for the trusted main publishing job

The `publish` job will retain `contents: write` because it creates the GitHub release. It runs only following a successful build on a trusted `main` push and does not execute untrusted fork PR code. `packages: write` and `id-token: write` remain scoped to this job for their existing publication functions.

**Alternative considered:** split release creation into a separate workflow or use another credential solely to remove the alert. Rejected because it adds cross-workflow artifact/credential complexity without reducing the necessary release authority.

### Treat alert #13 as a documented residual signal

Assurance evidence will record the alert number, required permission, trigger boundary, owner, validation evidence, and exit criterion. It will distinguish a necessary, constrained permission from a dismissed vulnerability. A future design may reconsider it if GitHub supports a narrower release-creation permission or release publishing changes.

### Verify Best Practices externally

A maintainer will submit the existing evidence through project 13670's supported Best Practices flow, then record the service timestamp, resulting badge level, and any outstanding criteria. The repository will preserve the real outcome, including an `in_progress` outcome if the service does not award Passing.

**Alternative considered:** update the README or local JSON to show Passing immediately. Rejected because the external project record is authoritative.

## Risks / Trade-offs

- [A write scope is accidentally omitted] → Validate all release, pre-release, changelog, package, and label paths after changing permissions; roll back only the missing scope, at the affected job boundary.
- [Scorecard continues to flag #13] → Preserve the scoped permission and publish the rationale/evidence; do not weaken controls merely to change scanner output.
- [Best Practices does not award Passing] → Record the actual result and the specific remaining required criteria rather than altering evidence to force a status.
- [Repository default setting is changed out of band] → Verify the GitHub Actions default workflow-permissions setting during implementation and record it in assurance evidence.

## Migration Plan

1. Change permissions while preserving the existing workflow triggers and job conditions.
2. Validate workflow YAML and exercise or inspect the affected release paths before relying on production release automation.
3. Set and verify the repository's read-only default workflow-token setting.
4. Submit Best Practices evidence, wait for the authoritative result, and update assurance documentation.
5. Trigger or await a fresh Scorecard analysis; record closed and remaining alerts.

Rollback consists of restoring the previous workflow permission declaration only for an affected automation failure, then narrowing the restored scope after the missing permission is identified. No application data or public API migration is required.

## Open Questions

- Which supported Best Practices submission method/account is available to the maintainer, and does it produce a Passing badge immediately after review?
- Does the next Scorecard version recognize the justified release-creation permission, or will #13 remain a documented residual alert?
