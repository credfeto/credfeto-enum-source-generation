# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.6.2271
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.4] - 2022-11-07
### Added
- Ability to generate extension methods for third party enums

## [0.0.3] - 2022-11-06
### Added
- Checks for enum.ToString where they should be enum.GetName

## [0.0.2] - 2022-11-06
### Added
- Unit tests on the code generated result
- Benchmarks to prove the code generated code executes faster
### Changed
- FF-1429 - Updated Meziantou.Analyzer to 1.0.745

## [0.0.1] - 2022-11-06
### Added
- Generator for GetName(this Enum value)
- Generator for GetDescription(this Enum value)
- Version of program to generated code attribute

## [0.0.0] - Project created