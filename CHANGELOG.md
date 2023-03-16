# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- FF-1429 - Updated Meziantou.Analyzer to 2.0.19
- FF-1429 - Updated FunFair.Test.Common to 6.0.26.2754
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.54.0.64047
- SDK - Updated DotNet SDK to 7.0.202
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [1.0.7] - 2023-02-25
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.24.2725
- FF-1429 - Updated Microsoft.CodeAnalysis.CSharp.Workspaces to 4.5.0

## [1.0.6] - 2023-02-24
### Changed
- FF-1429 - Updated Microsoft.CodeAnalysis.Analyzers to 3.3.4
- FF-1429 - Updated NSubstitute.Analyzers.CSharp to 1.0.16
- FF-1429 - Updated SmartAnalyzers.CSharpExtensions.Annotations to 4.2.8
- FF-1429 - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.5.22
- FF-1429 - Updated NSubstitute to 5.0.0
- FF-3881 - Updated DotNet SDK to 7.0.200
- FF-1429 - Updated FunFair.Test.Common to 6.0.23.2715
- FF-1429 - Updated Microsoft.NET.Test.Sdk to 17.5.0
- FF-1429 - Updated Meziantou.Analyzer to 2.0.18
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.53.0.62665

## [1.0.5] - 2023-01-17
### Changed
- FF-1429 - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.4.33
- FF-1429 - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.2.32
- FF-3881 - Updated DotNet SDK to 7.0.102
- FF-1429 - Updated FunFair.Test.Common to 6.0.15.2520
- FF-1429 - Updated BenchmarkDotNet to 0.13.4
- FF-1429 - Updated Meziantou.Analyzer to 2.0.8

## [1.0.4] - 2022-12-27
### Changed
- FF-1429 - Updated NonBlocking to 2.1.1

## [1.0.3] - 2022-12-26
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.12.2443
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.51.0.59060
- FF-1429 - Updated BenchmarkDotNet to 0.13.3

## [1.0.2] - 2022-12-16
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.11.2429
- FF-1429 - Updated Meziantou.Analyzer to 1.0.757
- FF-1429 - Updated Microsoft.NET.Test.Sdk to 17.4.1

## [1.0.1] - 2022-12-14
### Added
- IsDefined extension method as an optimised Enum.IsDefined alternative
### Changed
- FF-1429 - Updated Meziantou.Analyzer to 1.0.756
- FF-1429 - Updated Roslynator.Analyzers to 4.2.0
- FF-1429 - Updated xunit.analyzers to 1.1.0
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.50.0.58025
- FF-1429 - Updated FunFair.Test.Common to 6.0.10.2422
- FF-3881 - Updated DotNet SDK to 7.0.101

## [1.0.0] - 2022-11-22
### Added
- Additional tests for code coverage of the generator and generated code
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.8.2334
- FF-1429 - Updated Microsoft.CodeAnalysis.CSharp.Workspaces to 4.4.0
- FF-1429 - Updated Meziantou.Analyzer to 1.0.750

## [0.0.8] - 2022-11-09
### Changed
- Use nameof(enum field) rather than the string
- Extracted throwing exceptions to a separate method
- FF-1429 - Updated Meziantou.Analyzer to 1.0.746
- FF-1429 - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.4.27
- FF-1429 - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.2.30
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.48.0.56517
- FF-1429 - Updated FunFair.Test.Common to 6.0.7.2278

## [0.0.7] - 2022-11-08
### Changed
- FF-3881 - Updated DotNet SDK to 7.0.100

## [0.0.6] - 2022-11-08
### Fixed
- File not found issues when publishing on windows
### Changed
- FF-1429 - Updated Microsoft.Extensions to 7.0.0

## [0.0.5] - 2022-11-07
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.6.2271
- FF-1429 - Updated Microsoft.NET.Test.Sdk to 17.4.0
- Added CopyLocalLockFileAssemblies to project file to make it generate the correct package

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