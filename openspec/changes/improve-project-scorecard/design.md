## Context

The repository has four SharpFuzz/AFL++ parser targets, committed seed corpora, pull-request corpus replay, and a scheduled custom fuzz job. OpenSSF Scorecard's Fuzzing check recognizes specific integrations such as OSS-Fuzz and ClusterFuzzLite rather than arbitrary workflow names or direct `afl-fuzz` invocation, so the existing control can remain invisible. FrontierSharp is also registered as bestpractices.dev project 13670 and already has a README badge, but it lacks the repository-side `.bestpractices.json` automation input that makes criterion evidence reviewable with code changes.

The change spans GitHub Actions, fuzz-runner packaging, repository assurance data, and two OpenSSF services. It must improve machine detection without fabricating evidence, weakening least privilege, exposing secrets, or replacing deterministic corpus replay.

## Goals / Non-Goals

**Goals:**

- Run the existing parser fuzz targets through functioning coverage-guided SharpFuzz/AFL++ campaigns, whether or not the current OpenSSF Scorecard Fuzzing check recognizes them.
- Preserve hermetic inputs, explicit time limits, minimum permissions, deterministic pull-request replay, and actionable crash retention.
- Make every currently supportable Passing, Silver, and Gold Best Practices answer available in a valid root `.bestpractices.json` with repository evidence.
- Keep the README badge linked to project 13670 and driven by the service's actual status.
- Record reproducible verification for both external signals.

**Non-Goals:**

- Adding placeholder controls solely to increase a metric.
- Claiming Best Practices criteria that cannot be demonstrated by current repository or project evidence.
- Automatically overwriting the public bestpractices.dev project without maintainer review.
- Removing the existing corpus replay gate or changing public FrontierSharp APIs.
- Guaranteeing immediate external rescans, which remain subject to GitHub and OpenSSF scheduling.

## Decisions

### 1. Retain the operational SharpFuzz/AFL++ integration

Keep target selection and parser logic in `FrontierSharp.Fuzz` and retain the existing SharpFuzz/AFL++ engine. ClusterFuzzLite requires Clang/libFuzzer targets and has no documented .NET adapter, so adding its workflow would not run the existing harness truthfully. Hosted OSS-Fuzz has the same incompatibility and would introduce external admission and infrastructure requirements.

Run each existing target independently so that its 30-minute bound fits inside the job timeout. Do not add ClusterFuzzLite, OSS-Fuzz, or an unrelated property test merely to affect Scorecard. Scorecard `v5.5.0` adds FsCheck detection but not SharpFuzz detection; a future FsCheck property is appropriate only if it independently improves parser coverage.

### 2. Separate pull-request replay from bounded fuzz campaigns

Keep deterministic replay in normal pull-request CI. Run fuzz campaigns on schedule and manual dispatch with explicit per-target and job timeouts, read-only repository access, no secrets, and no publishing permissions. Upload only sanitized crash/reproducer artifacts with bounded retention.

This preserves fast, deterministic PR feedback while allowing the recognized integration to perform coverage-guided campaigns. Running full campaigns on every pull request was rejected because of cost, nondeterministic duration, and untrusted-fork risk.

### 3. Pin third-party workflow inputs and verify detection from emitted Scorecard data

Pin all actions and external build inputs to immutable commits or digests. Validate workflow syntax and a bounded target run locally where possible, then use a fresh Scorecard result/SARIF finding to prove that the Fuzzing check names the recognized integration. Record the run URL, date, Scorecard version, and relevant finding without copying credentials or unrelated alert data.

A workflow-text assertion may provide fast regression feedback, but it is not sufficient acceptance evidence because upstream detection rules can change.

### 4. Generate `.bestpractices.json` from the registered project's schema, then curate it

Start from the current JSON representation for bestpractices.dev project 13670 or the service's documented criterion keys. Store a valid root `.bestpractices.json` because that is the service-supported repository automation location. Include all known, truthful answers across Passing, Silver, and Gold and attach concise evidence URLs or explanations where the schema supports them. Use `?`/`unknown` or omit entries that cannot yet be supported, consistent with service semantics.

The file is evidence input, not proof that a badge level has been awarded. Changes require JSON/schema validation, link checks for repository-local evidence, and comparison with the current public project before a maintainer triggers bestpractices.dev automation.

### 5. Keep the README badge service-backed

Use the badge image endpoint and project page for project 13670 rather than a static shields.io level. This ensures the displayed level follows bestpractices.dev. Preserve the repository's compact badge row and add accessible alt text if needed.

## Risks / Trade-offs

- **Scorecard does not recognize the current .NET/SharpFuzz execution model** → retain and validate the operational AFL++ workflow, record the false negative, and do not add a metric-only configuration.
- **A future Scorecard detector may recognize another test framework** → evaluate it only when it adds useful coverage and is supported by a released Scorecard action.
- **Scheduled campaigns consume more CI time** → Apply per-target budgets, concurrency controls, and scheduled cadence appropriate to the repository.
- **Crash artifacts may contain corpus-derived data** → Use only non-sensitive seeds, sanitize artifact paths/content, and set bounded retention.
- **Best Practices answers can become stale or overstate compliance** → Require evidence per answer, preserve unknowns, compare with project 13670, and review the file alongside control changes.
- **External services update asynchronously** → Record pending verification separately and do not weaken controls or fabricate success while waiting.

## Migration Plan

1. Capture the current Fuzzing finding and project 13670 JSON as dated baselines.
2. Add and validate the ClusterFuzzLite adapter for one parser target while leaving existing replay and scheduled fuzzing operational.
3. Expand the recognized integration to all four targets, verify bounded execution and artifacts, then remove only redundant scheduled steps.
4. Add and validate `.bestpractices.json`; reconcile it with the public project through the documented maintainer-reviewed automation flow.
5. Confirm the README badge resolves to project 13670 and run a fresh Scorecard analysis.
6. Record external results and any residual limitations.

Rollback consists of reverting the recognized integration/configuration while restoring the previous scheduled AFL++ job. Corpus replay remains unchanged throughout. Revert individual `.bestpractices.json` claims if evidence is invalid; the public badge continues to display the service's actual state.

## Open Questions

- Which pinned ClusterFuzzLite release and runner/container combination successfully drives SharpFuzz on .NET 10?
- Does project 13670's exported JSON require normalization before it is accepted as repository automation input?
- Which Silver and Gold criteria are currently demonstrable, and which must remain unknown pending new controls or public evidence?
