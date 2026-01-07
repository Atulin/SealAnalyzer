[![.NET Test & Coverage](https://github.com/Atulin/SealAnalyzer/actions/workflows/test.yml/badge.svg)](https://github.com/Atulin/SealAnalyzer/actions/workflows/test.yml)
[![publish](https://github.com/Atulin/SealAnalyzer/actions/workflows/publish.yml/badge.svg)](https://github.com/Atulin/SealAnalyzer/actions/workflows/publish.yml)
[![codecov](https://codecov.io/gh/Atulin/SealAnalyzer/graph/badge.svg?token=E04Y3J9R04)](https://codecov.io/gh/Atulin/SealAnalyzer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![NuGet Downloads](https://img.shields.io/nuget/dt/Atulin.SealAnalyzer)


# Seal Analyzer

Make sure all your classes are sealed.

## Usage

Add the [Atulin.SealAnalyzer](https://www.nuget.org/packages/Atulin.SealAnalyzer/) nuggie to your project.

## Advanced usage

Add `[assembly: SealPublicClasses]` attribute to your assembly to enable the diagnostic on public classes