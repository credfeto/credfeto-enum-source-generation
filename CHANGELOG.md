# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- Dependencies - Updated SonarAnalyzer.CSharp to 9.4.0.72892
- Dependencies - Updated Meziantou.Analyzer to 2.0.62
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.6.3
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [1.0.13] - 2023-06-20
### Changed
- SDK - Updated DotNet SDK to 8.0.100-preview.5.23303.2

## [1.0.12] - 2023-06-13
### Changed
- Dependencies - Updated Meziantou.Analyzer to 2.0.61
- Dependencies - Updated SonarAnalyzer.CSharp to 9.3.0.71466
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.6.2

## [1.0.11] - 2023-06-02
### Changed
- Dependencies - Updated Meziantou.Analyzer to 2.0.56
- Dependencies - Updated SonarAnalyzer.CSharp to 9.2.0.71021
- Dependencies - Updated FunFair.Test.Common to 6.1.1.49
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.6.1

## [1.0.10] - 2023-05-21
### Changed
- Dependencies - Updated Roslynator.Analyzers to 4.3.0
- Dependencies - Updated SonarAnalyzer.CSharp to 9.0.0.68202
- Dependencies - Updated FunFair.Test.Common to 6.1.0.8
- Dependencies - Updated FunFair.CodeAnalysis to 7.0.0.18
- Dependencies - .NET 8 Preview 3
- Dependencies - Updated Meziantou.Analyzer to 2.0.52
- Dependencies - Updated Microsoft.CodeAnalysis.CSharp to 4.6.0
- Dependencies - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.6.40
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.6.0
- Dependencies - Updated CSharpIsNullAnalyzer to 0.1.495
- SDK - Updated DotNet SDK to 8.0.100-preview.4.23260.5
- Dependencies - Updated coverlet to 6.0.0

## [1.0.9] - 2023-04-23
### Changed
- SDK - Updated DotNet SDK to 7.0.203
- Dependencies - Updated Meziantou.Analyzer to 2.0.36
- Dependencies - Updated SonarAnalyzer.CSharp to 8.56.0.67649
- Dependencies - Updated FunFair.Test.Common to 6.0.29.1

## [1.0.8] - 2023-03-21
### Changed
- FF-1429 - Updated Meziantou.Analyzer to 2.0.19
- FF-1429 - Updated FunFair.Test.Common to 6.0.26.2754
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.54.0.64047
- SDK - Updated DotNet SDK to 7.0.202
- Dependencies - Updated Meziantou.Analyzer to 2.0.22
- Dependencies - Updated SonarAnalyzer.CSharp to 8.55.0.65544

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