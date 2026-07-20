## Context

The current Scorecard run reports eleven alerts, but six of them are repeated instances of two underlying dependency problems: a mutable Docker base tag and unlocked NuGet restores. Two Docker alerts point at local build stages rather than external images. The remaining findings concern branch/review governance, fuzzing, security-policy link detection, and OpenSSF Best Practices registration. A revised SharpFuzz/.NET 10 compatibility spike succeeded by using a framework-dependent harness, instrumenting a separate pure-IL target assembly, and running AFL++ with ReadyToRun and tiered compilation disabled.

The repository already pins GitHub Actions, runs build/test and dependency-review checks, enables CodeQL and private vulnerability reporting, uses a no-bypass `mainline` ruleset, and maintains a threat model. It is operated by one collaborator, so independent approval and CODEOWNERS requirements cannot be enabled without blocking legitimate maintainer and Dependabot work. The selected completion model is therefore control installation plus verified residual-risk evidence, not an artificial promise of zero immediately open alerts.

The implementation crosses Docker, NuGet/MSBuild, .NET parser testing, GitHub Actions, repository-admin settings, and public security documentation.

## Goals / Non-Goals

**Goals:**

- Make container and NuGet inputs deterministic and reviewable.
- Exercise security-relevant untrusted-input parsers with coverage-guided fuzzing and replayable regression inputs.
- Maximize enforceable branch and CI protections without creating an unusable solo-maintainer workflow.
- Give vulnerability reporters a direct private channel and realistic coordination expectations.
- Produce truthful Best Practices and Scorecard evidence, including explicit owners and exit criteria for residual findings.
- Preserve existing public library and CLI behavior.

**Non-Goals:**

- Creating fake reviewers, approvals, CODEOWNERS identities, badges, or fuzzing evidence to manipulate Scorecard.
- Claiming immediate closure of the history-derived Code-Review finding.
- Adopting a second maintainer or changing the selected solo-maintainer operating model.
- Fuzzing live EVE Frontier endpoints, arbitrary host files, or the CLI as an unbounded subprocess.

## Decisions

### 1. Treat the alert set as six remediation tracks with evidence-based completion

The change will map each alert number to dependency reproducibility, parser robustness, repository controls, disclosure, Best Practices evidence, or residual governance risk. Completion requires the relevant control to be installed and verified, or a residual record to identify why closure is external, temporal, or a proven scanner limitation.

**Alternative considered:** keep the change open until the security dashboard reaches zero alerts. This was rejected because the Code-Review check samples historical approved changesets and cannot be repaired by a source change, while the solo-maintainer decision precludes meaningful independent approvals.

### 2. Pin one external Docker input and remove ambiguous alias stages

`BASE_IMAGE` will retain the readable `mcr.microsoft.com/dotnet/runtime-deps:10.0` tag and append a reviewed multi-architecture manifest digest. The Dockerfile will use a single external `FROM` stage and copy the selected `linux-${TARGETARCH}` publish output directly, eliminating `FROM base` alias lines that Scorecard reports as additional images.

The selected digest must contain both `linux/amd64` and `linux/arm64`. Dependabot's existing Docker configuration remains the update mechanism, so digest changes arrive as reviewable pull requests instead of silently changing builds.

**Alternative considered:** pin only the existing `BASE_IMAGE` value and dismiss both alias alerts immediately. This preserves valid Docker semantics but leaves ambiguous scanner output. Structural simplification is preferred; dismissal is allowed only after a post-change scan still demonstrates a false positive.

### 3. Lock the complete NuGet graph used by the main solution

The main `src` build will enable lock-file generation through shared MSBuild configuration and commit the generated `packages.lock.json` files for every project restored through the solution. The package-validation script and both workflow restore steps will use `dotnet restore --locked-mode`; subsequent build, test, and pack steps already avoid implicit restore.

Lock files provide transitive versions and package content hashes, while locked mode turns unreviewed graph drift into a build failure. Dependabot updates must be verified to refresh project references and lock files together. The separately maintained example project is included only if its normal validation path restores it; otherwise it remains outside this alert-remediation scope and is recorded explicitly.

**Alternative considered:** rely on exact direct `PackageReference` versions. This does not freeze the transitive graph. Setting locked-mode properties without committing generated lock files was also rejected because clean restores would fail without a reviewable graph.

### 4. Isolate SharpFuzz targets from production and publishing

A dedicated framework-dependent `net10.0` fuzzing project will reference production assemblies but instrument only separate pure-IL target assemblies. The runner will use SharpFuzz `2.3.0`, AFL++, `DOTNET_ReadyToRun=0`, `DOTNET_TieredCompilation=0`, and `DOTNET_ROLL_FORWARD=Major`. This model produced coverage without crashes in the compatibility spike and does not join the publishing path.

Targets cover Sui JSON converters, `ResIndex`, Razorvine pickle decoding, and World API JSON through hermetic fixtures. Normal CI replays committed corpora; a separate bounded workflow runs coverage-guided fuzzing.

### 5. Use a truthful solo-maintainer ruleset baseline

The `mainline` ruleset will continue to require pull requests, signed commits, linear history, required build/dependency checks, code scanning, resolved review threads, stale-review dismissal, and no bypass actors. Required status checks will be made strict/up-to-date where GitHub permits.

The currently ineffective code-owner-review requirement will not be represented as protection while no valid CODEOWNERS identity exists. Required approvals and last-push approval will remain disabled because only the author/administrator is available. The governance evidence will state that independent review is an accepted residual risk and define onboarding a trusted second reviewer as the exit criterion.

**Alternative considered:** add `@Scetrov` as sole CODEOWNER and require approval. GitHub does not permit meaningful self-approval, so this can deadlock maintainer and Dependabot pull requests without adding independent assurance.

### 6. Separate disclosure, badge, and scanner evidence

`SECURITY.md` will link directly to GitHub private vulnerability reporting and state maintainer-approved numeric acknowledgement, update, and coordinated-disclosure targets. The Best Practices badge or link will be published only after the repository is registered and its actual status can be resolved publicly.

A post-change evidence record will capture the Scorecard version/date, alert outcomes, validation commands, any justified false-positive dismissals, and residual findings. Code-review history, solo governance, and the deferred fuzzing toolchain failure are recorded separately from installed controls.

## Risks / Trade-offs

- **Pinned Docker digests can delay upstream security fixes** → Keep weekly Dependabot Docker updates and review/rebuild promptly.
- **The selected digest may omit a target architecture** → Inspect the manifest and build both supported platforms before merge.
- **Docker simplification may change BuildKit path interpolation behavior** → Build both architectures from clean publish outputs and retain the prior Dockerfile as the rollback reference.
- **Lock files increase generated diff and merge-conflict volume** → Keep generation deterministic, document refresh commands, and verify Dependabot behavior with a test update.
- **Locked restore can break when any manifest changes without refreshed locks** → Fail early in CI and provide a documented local refresh workflow.
- **SharpFuzz does not currently support the tested .NET 10/container combination** → Preserve the failed spike evidence and require a dedicated follow-up change to establish a working coverage-guided runner before adding harnesses.
- **Strict status checks can increase merge latency** → Keep only stable security-relevant checks required and diagnose flaky checks rather than bypassing them.
- **Scorecard alerts may remain despite meaningful controls** → Preserve scan evidence and use dismissal only for reproduced scanner errors, never for convenience.
- **Public response timelines can create unrealistic expectations** → Use explicit targets rather than contractual service-level guarantees and obtain maintainer approval before publication.

## Migration Plan

1. Capture the current Scorecard alert set and current ruleset configuration as baseline evidence.
2. Record the failed SharpFuzz compatibility spike and verify the Docker manifest/platform assumptions before broad file changes.
3. Add NuGet lock generation, commit cleanly generated locks, and switch all flagged restores to locked mode.
4. Pin and simplify the Dockerfile, then validate clean multi-platform builds.
5. Update `SECURITY.md`, complete real Best Practices registration, and update only the solo-safe ruleset settings.
6. Rerun build, test, pack, container, and Scorecard validation; record closures and residuals.

Rollback consists of reverting the source/workflow changes and restoring the prior ruleset JSON. Digest and lock-file rollback must restore the matching previously reviewed dependency state rather than reverting to mutable or unlocked inputs. External badge registration need not be deleted; its status can remain truthful while a broken badge link is removed.

## Open Questions

- Which current `runtime-deps:10.0` manifest digest is approved at implementation time?
- What numeric acknowledgement, update, and coordinated-disclosure targets does the maintainer approve for `SECURITY.md`?
- What bounded PR and scheduled fuzzing durations fit the repository's Actions budget?
