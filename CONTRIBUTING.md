# Contributing to FrontierSharp

Thank you for helping improve FrontierSharp. By contributing, you agree to follow this guide and the project's [security policy](SECURITY.md).

## Before you start

- Discuss substantial changes in an issue before investing significant implementation effort.
- Report suspected vulnerabilities through the [private reporting channel](SECURITY.md#reporting-a-vulnerability), not in a public issue or pull request.
- Use a focused branch from `main`; do not commit generated binaries, credentials, or local configuration.

## Development standard

Contributions must:

- keep the build warning-free (`TreatWarningsAsErrors` is enabled);
- include focused unit tests for new behavior and regressions;
- maintain or improve relevant code coverage, including error paths; and
- update public documentation, changelog entries under **Unreleased**, and any affected examples.

Run the same essential checks as CI from the repository root:

```sh
dotnet restore --locked-mode ./src/FrontierSharp.sln
dotnet build ./src/FrontierSharp.sln --no-restore --configuration Release
dotnet test ./src/FrontierSharp.sln --no-build --configuration Release \
  --collect:"XPlat Code Coverage" --results-directory ./src/TestResults
```

The coverage reports are written below `src/TestResults`. Review coverage for the code you changed rather than treating a percentage alone as a quality target.

## Commits and pull requests

- Sign every commit with your verified GPG or SSH signing key (`git commit -S`).
- Use a concise, imperative commit subject. Add a body when the reason for a change is not obvious.
- Open a pull request against `main` with a clear summary, testing evidence, and linked issue when applicable.
- Keep the pull request small and reviewable. Respond to CI and review feedback before merge.
- Do not merge a pull request until required checks pass and it has the required approval under the repository's branch-protection rules.

## Interim builds and releases

For pull requests from branches in this repository, CI publishes a GitHub **pre-release** named for the pull request and applies the `pre-release` label. The release contains packages built from the current pull-request commit and is replaced when the pull request is updated. These builds are for evaluation only; use a stable GitHub Release or NuGet package in production.

After a stable release is published, automation copies its GitHub release notes into [CHANGELOG.md](CHANGELOG.md). Contributors should still add human-readable entries under **Unreleased** so the project history remains clear before release automation runs.
