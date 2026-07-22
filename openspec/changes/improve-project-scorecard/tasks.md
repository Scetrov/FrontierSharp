## 1. Capture OpenSSF Baselines

- [x] 1.1 Record the current Scorecard Fuzzing finding, analysis version, run date, and code-scanning alert URL in dated non-sensitive evidence
- [x] 1.2 Download project 13670's current bestpractices.dev JSON and inventory answered, unknown, and unsupported criteria across Passing, Silver, and Gold
- [x] 1.3 Resolve and record the pinned ClusterFuzzLite action revision and runner/container inputs to evaluate with .NET 10 and SharpFuzz

## 2. Retain Operational SharpFuzz/AFL++ Fuzzing

- [x] 2.1 Assess a recognized integration for one existing parser target and document why a functional ClusterFuzzLite adapter is not currently feasible
- [x] 2.2 Document the Scorecard SharpFuzz/AFL++ false negative and retain the existing operational fuzzer without claiming detection success
- [x] 2.3 Configure target-scoped scheduled and manual fuzz campaigns with explicit bounds, read-only permissions, no secrets, and no publishing access
- [x] 2.4 Retain the existing AFL++ workflow and document the blocker without claiming Scorecard detection

## 3. Maintain All Parser Targets

- [x] 3.1 Run Sui, ResIndex, Pickle, and World as independent AFL++ campaigns using their committed non-sensitive corpora
- [x] 3.2 Preserve pull-request corpus replay and add concurrency controls plus bounded crash/hang artifact retention
- [x] 3.3 Validate workflow syntax, all deterministic corpora, bounded fuzz execution, and crash artifact replay

## 4. Publish Best Practices Evidence

- [x] 4.1 Create a valid root `.bestpractices.json` using documented criterion keys and truthful answers from project 13670 and repository evidence
- [x] 4.2 Cover every currently answerable Passing, Silver, and Gold criterion, leaving unsupported entries omitted or explicitly unknown
- [x] 4.3 Validate JSON syntax, accepted automation semantics, and repository-local evidence links, then compare the result with the public project record
- [x] 4.4 Ensure the README displays the service-backed OpenSSF Best Practices badge linked to project 13670 with accurate accessible text

## 5. Verify External Signals

- [ ] 5.1 Trigger a fresh Scorecard analysis and record the version, date, run URL, finding, and any continuing SharpFuzz/AFL++ detection limitation
- [ ] 5.2 Submit or trigger the documented maintainer-reviewed bestpractices.dev automation flow and confirm the badge continues to show the service-awarded status
- [x] 5.3 Record asynchronous or residual findings with an owner and exit criterion instead of fabricating successful external results
- [x] 5.4 Run focused project validation and `openspec validate --change improve-project-scorecard`, then update the evidence record with final outcomes
