## Why

FrontierSharp already runs bounded parser fuzzing and publishes OpenSSF assurance information, but Scorecard does not recognize the custom fuzzing integration and the Best Practices evidence is not maintained in a repository-readable form. Aligning these signals with supported OpenSSF integrations will make the score reflect real controls while keeping every badge claim and criterion answer auditable.

## What Changes

- Retain the custom SharpFuzz/AFL++ scheduled fuzz job, split it into independent bounded target campaigns, and preserve the existing .NET parser targets, deterministic corpus replay, least privilege, and bounded execution.
- Record the OpenSSF Scorecard Fuzzing false negative through the repository's normal Scorecard/code-scanning path without representing that detector limitation as an absence of fuzzing.
- Present the registered OpenSSF Best Practices project as a status badge in the README, linked to the actual FrontierSharp project record.
- Add a root `.bestpractices.json` containing truthful, repository-backed answers and evidence for every currently answerable Passing, Silver, and Gold criterion; leave unsupported or unknown claims unanswered rather than overstating compliance.
- Document how the evidence file is validated against the Best Practices automation format and kept synchronized with the public project record.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `parser-robustness-assurance`: Require the operational fuzzing workflow to use a Scorecard-recognized integration without weakening the existing hermetic, bounded parser fuzzing guarantees.
- `repository-security-governance`: Require auditable `.bestpractices.json` evidence across Passing, Silver, and Gold criteria and a truthful README badge linked to the registered project.

## Impact

- Affected automation: `.github/workflows/fuzz.yml` and its SharpFuzz/AFL++ artifact handling; no decorative ClusterFuzzLite/CIFuzz configuration is added.
- Affected assurance data and documentation: root `.bestpractices.json`, `README.md`, and post-change Scorecard/Best Practices verification evidence.
- External systems: GitHub Actions, GitHub code scanning, OpenSSF Scorecard, and bestpractices.dev project 13670.
- No public library, CLI, or network API behavior changes are expected.
