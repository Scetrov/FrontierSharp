## ADDED Requirements

### Requirement: Immutable multi-platform container base
The command-line container build SHALL resolve its external .NET runtime base image by a full SHA-256 manifest digest while retaining a readable version tag, and the selected manifest MUST support every platform published by the workflow.

#### Scenario: Supported container platforms resolve one reviewed digest
- **WHEN** the command-line container is built for `linux/amd64` and `linux/arm64`
- **THEN** both builds resolve the reviewed digest declared in the Dockerfile and complete without falling back to a mutable tag

#### Scenario: External image input is auditable
- **WHEN** a reviewer inspects every external Dockerfile `FROM` input
- **THEN** each external image reference contains a full `@sha256:` digest and no local build-stage alias is misrepresented as an external dependency

### Requirement: Reviewable container digest updates
The repository SHALL keep the command-line Docker dependency under automated update management so a changed upstream digest is proposed as a reviewable pull request rather than adopted silently.

#### Scenario: Upstream runtime image changes
- **WHEN** the configured .NET runtime tag points to a newer manifest digest
- **THEN** the dependency update service proposes the digest change for review and the existing multi-platform validation runs before merge

### Requirement: Committed NuGet dependency graphs
The repository SHALL generate and commit NuGet lock files for every project restored through the main solution so the resolved transitive graph and package content hashes are reviewable.

#### Scenario: Clean solution restore
- **WHEN** the main solution is restored from a clean checkout
- **THEN** every restored project has a committed lock file that describes the resolved dependency graph

#### Scenario: Dependency manifest changes
- **WHEN** a direct or transitive dependency graph change is intended
- **THEN** the corresponding project manifest and lock-file changes are reviewed together

### Requirement: Locked restore enforcement
Local package validation and every CI build or publish restore SHALL run NuGet in locked mode, and subsequent build, test, and pack commands MUST NOT perform an implicit unlocked restore.

#### Scenario: Lock files match project manifests
- **WHEN** the package-validation script or CI executes a restore with unchanged manifests and lock files
- **THEN** locked restore succeeds and the remaining build, test, and pack steps use that restored graph

#### Scenario: Dependency graph drifts
- **WHEN** a project manifest or transitive resolution no longer matches its committed lock file
- **THEN** locked restore fails before build, packaging, or publication begins

### Requirement: Lock maintenance remains automated
The existing dependency update service SHALL remain able to update NuGet project references and their affected lock files without disabling locked-mode enforcement.

#### Scenario: Automated NuGet update
- **WHEN** the dependency update service proposes a supported NuGet package update
- **THEN** the pull request includes the required lock-file refresh and passes the same locked restore used by normal CI
