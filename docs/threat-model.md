# FrontierSharp security threat model

## Scope

FrontierSharp is a .NET library and command-line client that retrieves and
processes EVE Frontier World API and static-data responses. The repository also
builds NuGet packages, command-line archives, and a container image through
GitHub Actions.

## Assets

- Source code, dependency definitions, and CI workflow definitions.
- NuGet packages, GitHub Packages, release archives, and container images.
- Release credentials and GitHub Actions tokens used only during publishing.
- API responses, static-data files, and user-supplied command-line options.

## Trust boundaries

- GitHub pull requests and workflow inputs are untrusted until reviewed.
- GitHub-hosted runners cross from repository source to package and release
  publishing services.
- EVE Frontier API and static-data responses are external input and must be
  treated as untrusted until parsed and validated by the client.
- NuGet.org, GitHub Packages, GHCR, and their consumers receive published
  artifacts outside the repository boundary.

## Primary threats and controls

| Threat | Control |
| --- | --- |
| Compromised or unexpectedly updated CI action | Pin actions to reviewed full commit SHAs and review Dependabot updates. |
| Dependency update introduces vulnerable or unwanted package | Run dependency review on pull requests and keep Dependabot version updates enabled. |
| Workflow change gains excessive token access | Use an explicit read-only workflow permission baseline and grant publish permissions only to the publish job. |
| Unauthorized release publication or artifact substitution | Restrict publishing to the protected default branch, retain provenance/SBOM generation, and require release-environment approval after maintainers configure it. |
| Malformed remote API or static-data input | Preserve parsing, error handling, and test coverage when changing request or serialization code. |
| Accidental credential disclosure | Use GitHub secrets only for legacy publishing credentials, keep secret scanning and push protection enabled, and never log credential values. |

## Security-sensitive changes

Changes to `.github/workflows/`, dependency manifests, release scripts,
`SECURITY.md`, and this threat model require maintainer review. Maintainers
must add valid GitHub users or teams to `CODEOWNERS` before enabling ownership
mappings; no identities are inferred by this document.

## Review triggers

Revisit this model when adding a new publisher, registry, external API,
credential, privileged workflow, deserialization format, or release artifact.
