# OpenSSF assurance evidence

This record captures non-sensitive, reproducible evidence for FrontierSharp's
OpenSSF Scorecard and Best Practices integrations. External services refresh
asynchronously; entries are evidence of the recorded observation, not a claim
about a later result.

## Scorecard baseline — 2026-07-21

- **Repository:** [`Scetrov/FrontierSharp`](https://github.com/Scetrov/FrontierSharp)
- **Observed analysis:** [online SCM analysis 1501677970](https://api.github.com/repos/Scetrov/FrontierSharp/code-scanning/analyses/1501677970)
- **Run date:** 2026-07-20T16:13:25Z
- **Scorecard version:** `v5.3.0`
- **Analysed revision:** `89abee3830f9fc3a5a45ab73826656de3f38a89a`
- **Fuzzing alert:** [FuzzingID alert 11](https://github.com/Scetrov/FrontierSharp/security/code-scanning/11)
- **Finding:** `score is 0: project is not fuzzed` / `Warn: no fuzzer integrations found`

OpenSSF Scorecard `v5.3.0` does not recognize the repository's SharpFuzz/AFL++
integration, so its `FuzzingID` result is a detector limitation rather than an
absence of parser fuzzing. The repository retains the functioning AFL++
workflow and does not add a decorative ClusterFuzzLite or OSS-Fuzz configuration
solely to change this score.

Scorecard `v5.5.0` adds FsCheck detection, but not SharpFuzz detection. An
`FsCheck.Xunit` parser property should be added only when it provides useful,
independent coverage; its possible future detector recognition is not a reason
to add it. Detection also depends on a released `scorecard-action` embedding
Scorecard `v5.5.0` or later.

## Best Practices baseline — 2026-07-21

- **Project:** [FrontierSharp 13670](https://www.bestpractices.dev/projects/13670)
- **Source:** [`projects/13670.json`](https://www.bestpractices.dev/projects/13670.json)
- **Downloaded:** 2026-07-21
- **Public record updated:** 2026-07-20T16:05:52.114Z
- **Badge status:** `in_progress` (Passing 94%, Silver 7%, Gold 17%)

The downloaded record contains 196 criterion status fields: 45 `Met`, 13
`N/A`, 9 `Unmet`, 128 unknown (`?`), and one unsupported numeric value
(`OSPS-BR-01.02_status: 0`). The `Met`, `N/A`, and `Unmet` entries are the
currently answered inventory; unknown and unsupported entries are not claims.
The public record includes both legacy Best Practices criteria and newer OSPS
fields. Any repository automation input must use only keys and values accepted
by the service at submission time, and must retain unknown criteria as unknown
or omit them.

## ClusterFuzzLite compatibility assessment — 2026-07-21

- **Candidate action revision:** [`google/clusterfuzzlite` `v1`](https://github.com/google/clusterfuzzlite/tree/82652fb49e77bc29c35da1167bb286e93c6bcc05), pinned as commit `82652fb49e77bc29c35da1167bb286e93c6bcc05`
- **Candidate runner:** GitHub-hosted `ubuntu-latest`
- **Candidate build container:** `gcr.io/oss-fuzz-base/base-builder:v1` (the documented ClusterFuzzLite base builder)
- **Application inputs:** .NET SDK 10.x and the existing SharpFuzz/AFL++ harnesses

The documented `build_fuzzers` and `run_fuzzers` actions require Linux Clang
**libFuzzer** targets, built in the OSS-Fuzz Docker toolchain and emitted as
self-contained binaries. Their documented language values are C/C++, Go, Rust,
Python, JVM, and Swift; .NET is not supported. SharpFuzz's current harnesses
run under the .NET runtime using AFL++, so they cannot satisfy those required
libFuzzer build and execution inputs without replacing the fuzzing engine.

This is a compatibility finding, not a detection-success claim. The existing
AFL++ workflow remains the operational fuzzing control unless a supported
adapter can be demonstrated. The existing `FrontierSharp.Fuzz` target is a
`net10.0` executable that calls `SharpFuzz.Fuzzer.Run`; it is neither a Clang
libFuzzer binary nor one of ClusterFuzzLite's documented language adapters.

## SharpFuzz instrumentation — 2026-07-21

- **SharpFuzz.CommandLine version:** `2.3.0` (pinned in `.github/workflows/fuzz.yml`)
- **AFL++ version tested:** `afl-fuzz++4.09c` (Ubuntu packaged)
- **Published harness path:** `src/FrontierSharp.Fuzz/bin/Debug/net10.0/publish/FrontierSharp.Fuzz.dll`

SharpFuzz requires an explicit instrumentation pass over the **parser dependency
assemblies** after publish. The proven workflow step is:

```bash
dotnet tool install --tool-path /tmp/sharpfuzz SharpFuzz.CommandLine --version 2.3.0
publish_directory=src/FrontierSharp.Fuzz/bin/Debug/net10.0/publish
for assembly in "$publish_directory"/*.dll; do
  case "$(basename "$assembly")" in
    FrontierSharp.Fuzz.dll|SharpFuzz.dll|SharpFuzz.Common.dll|System.*.dll) continue ;;
  esac
  /tmp/sharpfuzz/sharpfuzz "$assembly"
done
```

AFL++ must be invoked with:

```bash
export DOTNET_ReadyToRun=0
export DOTNET_TieredCompilation=0
export DOTNET_ROLL_FORWARD=Major
export AFL_SKIP_BIN_CHECK=1
export AFL_SKIP_CPUFREQ=1
export AFL_I_DONT_CARE_ABOUT_MISSING_CRASHES=1
afl-fuzz -V <seconds> -m none -i <corpus> -o <output> -- dotnet "<publish>/FrontierSharp.Fuzz.dll" fuzz <target>
```

### Observed coverage-guided executions — 2026-07-21

After instrumenting dependencies, AFL++ ran each parser target with its
committed corpus in the `mcr.microsoft.com/dotnet/sdk:10.0` container:

| Target   | Campaign duration | Executions (execs_done) |
|----------|-------------------|-------------------------|
| `sui`    | 5s                | 60,096                  |
| `resindex`| 5s               | 23,630                  |
| `world`  | 5s                | 20,628                  |
| `pickle` | 5s                | 57 (campaign interrupted after unexpected failure) |

`-m none` is required because SharpFuzz uses shared memory segments that can
exceed AFL++'s default 256 MiB `setrlimit`. The Pickle target may produce a
real unexpected parser failure during short bounded campaigns; such findings
are intentionally captured as sanitized crash artifacts for replay.

### Scorecard detection

SharpFuzz/AFL++ remains unrecognized by Scorecard `v5.3.0` (and Scorecard
`v5.5.0` detects FsCheck, not SharpFuzz). The repository retains the
functioning, proven AFL++ integration and does not claim detection success.

## Best Practices evidence — 2026-07-21

- **Local file:** `.bestpractices.json` at root
- **Public record:** [project 13670](https://www.bestpractices.dev/projects/13670)
- **Badge status:** `in_progress` (Passing 94%, Silver 7%, Gold 17%)
- **Criteria included:** 67 answerable (45 Met, 13 N/A, 9 Unmet)
- **Unknown criteria:** 128 omitted (not fabricated)
- **Mismatch with public record:** 0

The local evidence file uses the documented criterion key naming convention,
preserves the public project's truthful status values, and includes
repository-local evidence URLs for all answerable Met and N/A criteria.
Unsupported or unknown criteria remain omitted.

## Post-merge external verification — 2026-07-21

The PR `improve-project-scorecard` (github.com/Scetrov/FrontierSharp/pull/131)
has been merged. External services have not yet refreshed against the merged
commit; all observations below record the pending state without fabricating
results.

- **Scorecard (5.1):**
  - Latest recorded analysis: `2026-07-20T16:13:25Z`, commit `89abee3830f9`,
    Scorecard `v5.3.0` — predates the merge.
  - Fuzzing alert 11 (`FuzzingID`): `status: dismissed` at
    `2026-07-21T20:56:18Z`. The alert was dismissed during development
    because the proven AFL++ integration was documented and the false
    negative is recognised.
  - Next scheduled Scorecard analysis will run on the default-branch cron
    (`17 14 * * 1`, i.e. Monday 2026-07-27 at 14:17 UTC). That analysis
    will evaluate the merged code. The SharpFuzz/AFL++ finding is expected
    to remain unrecognised by `v5.3.0`; Scorecard `v5.5.0` adds FsCheck
    detection but not SharpFuzz, so no change in the Fuzzing signal is
    expected until a later Scorecard release or FsCheck properties are
    added.
  - **Owner:** maintainer — revisit after Monday cron or trigger analysis
    manually.
  - **Exit criterion:** record the new Scorecard commit SHA, date, and
    finding in this evidence file; no fabricated detection claim.

- **Best Practices badge (5.2):**
  - Current public record at `bestpractices.dev/projects/13670`:
    `badge_level: in_progress`, `badge_percentage_0: 94`,
    `updated_at: 2026-07-20T16:05:52.114Z` — pre-merge snapshot.
  - The root `.bestpractices.json` was committed in the merge but
    bestpractices.dev does not automatically read repository-local
    automation files; the maintainer must submit it through the
    documented automation flow on the project page.
  - The README badge image already resolves to project 13670, and the
    public status remains `in_progress` until the maintainer completes
    that submission.
  - **Owner:** maintainer — submit `.bestpractices.json` via
    bestpractices.dev project 13670 automation page.
  - **Exit criterion:** record the new `updated_at` timestamp and any
    badge-level change in this file; no fabricated status change.

Both items are recorded as pending with clear owners and exit criteria;
no external result has been fabricated.
