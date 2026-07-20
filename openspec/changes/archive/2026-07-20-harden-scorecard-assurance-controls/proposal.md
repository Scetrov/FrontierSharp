## Why

FrontierSharp has eleven open OpenSSF Scorecard alerts covering repository governance, dependency reproducibility, fuzz testing, security disclosure, and assurance evidence. The repository should install proportionate, verifiable controls now while documenting the Scorecard findings that cannot honestly close under the selected solo-maintainer operating model.

## What Changes

- Pin the command-line container base image to a reviewed multi-architecture digest and remove Docker stage syntax that Scorecard misclassifies as additional mutable dependencies.
- Commit the applicable NuGet dependency lock files and require locked restores in local package validation and both CI restore paths.
- Add coverage-guided SharpFuzz harnesses, minimized seed corpora, deterministic corpus replay, and bounded fuzzing for the highest-risk network JSON, static-index, and pickle parsing boundaries using a framework-dependent harness that instruments separate pure-IL targets.
- Strengthen the solo-maintainer branch ruleset without introducing unusable approval or CODEOWNERS requirements, and record the independently reviewed-change residual risk.
- Improve the security policy with a direct private-reporting link and explicit response and coordinated-disclosure expectations.
- Register truthful OpenSSF Best Practices evidence, rerun Scorecard after remediation, and record false-positive, external, or temporal residual alerts with owners and exit criteria.
- No public library or command-line API is intentionally changed.

## Capabilities

### New Capabilities

- `dependency-reproducibility`: Immutable container inputs and lock-enforced NuGet dependency resolution for validation, CI, and publishing.
- `parser-robustness-assurance`: Coverage-guided fuzzing and deterministic regression replay for security-relevant untrusted-input parsers.
- `repository-security-governance`: Solo-maintainer repository controls, coordinated vulnerability disclosure, truthful assurance evidence, and residual-risk tracking.

### Modified Capabilities

None.

## Impact

- Affected files include the command-line Dockerfile, NuGet restore configuration and generated lock files, package-validation and GitHub Actions workflows, security/governance documentation, remediation evidence, and a new fuzzing project, corpus, and workflow files.
- GitHub repository ruleset configuration and OpenSSF Best Practices registration require maintainer-admin actions outside the source tree.
- Dependabot must continue maintaining Docker digests, NuGet versions, and generated lock files.
- CI gains deterministic lock validation, multi-platform container verification, corpus replay, and bounded scheduled fuzzing, increasing maintenance and runner usage.
- Scorecard alert closure remains evidence-driven: code-review history, independent-approval findings may remain open with documented ownership and exit criteria.
