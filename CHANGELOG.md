# Changelog

All notable changes to FrontierSharp are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Added

- Contributor guide defining signed-commit, pull-request, unit-test, and coverage expectations.
- Pull-request GitHub pre-releases with evaluation packages and an explicit `pre-release` label.
- Automated synchronization of published GitHub Release notes into this changelog.
- CI coverage collection and report artifacts.

### Changed

- Improved OpenSSF Scorecard assurance signals: proven SharpFuzz/AFL++ fuzzing workflow, `.bestpractices.json` evidence, and fixed Best Practices badge.

## [1.0.36] - 2026-07-22

### Changed

- Archived the `improve-project-scorecard` open spec change and synced spec updates.

## [1.0.35] - 2026-07-22

### Changed

- Recorded post-merge external verification state for OpenSSF assurance signals.

## [1.0.34] - 2026-07-22

### Added

- Per-target bounded AFL++ fuzzing workflow with SharpFuzz instrumentation.
- `.bestpractices.json` with 67 answerable OpenSSF Best Practices criteria from public project record.

### Changed

- Fixed Best Practices badge URL and accessible alt text in README.

## [1.0.33] - 2026-07-20

### Added

- OpenSSF Scorecard supply-chain security workflow.
- Supply-chain workflows and governance documentation.

## [1.0.32] - 2026-07-20

### Changed

- Bumped NuGet dependencies (13 updates).
- Bumped GitHub Actions dependencies (4 updates).

## [1.0.30] - 2026-07-19

### Added

- Supply-chain security hardening controls: Dependabot, dependency review, and signed commits.
- Governance documentation and threat model.

## [1.0.29] - 2026-07-13

### Changed

- Bumped NuGet and GitHub Actions dependencies.

## [1.0.25] - 2026-07-06

### Added

- Command-line tool for interacting with World and Sui APIs.

## [1.0.14] - 2026-04-20

### Fixed

- Corrected null-handling in Sui GraphQL client edge cases.

### Changed

- Bumped NuGet minor/patch dependencies.

## [1.0.10] - 2026-04-14

### Added

- Fuzzing test project (`FrontierSharp.Fuzz`) with SharpFuzz/AFL++ integration for parser targets.
- Deterministic corpus replay for Sui, ResIndex, Pickle, and World parser targets.

## [1.0.9] - 2026-04-14

### Added

- NuGet package publishing workflow for releases.

## [1.0.7] - 2026-04-14

### Added

- Build and test CI workflow.
- Starmap data module for celestial body mappings.

## [1.0.2] - 2026-03-11

### Added

- World API client for tribes, solar systems, and types.

### Changed

- Bumped FluentResults dependency.

## [1.0.1] - 2026-03-11

### Added

- Sui GraphQL client for character data and killmails.
- HTTP client abstraction with caching support.

## [1.0.0] - 2026-03-09

### Added

- Initial stable release with World API and Sui GraphQL client libraries.
- EVE Frontier Cycle 5 data model support.
