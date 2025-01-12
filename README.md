> [!NOTE]
> This repository is in the process of being archived, as we move the codebases within to their own repositories.
> 
> ℹ️ Bunny has already moved [here](https://github.com/Health-Informatics-UoN/hutch-bunny)
>
> The other codebases will be moving soon, so beware that issues and pull requests are subject to these pending changes.

![Hutch][hutch-logo]

# Hutch Cohort Discovery ![MIT License][license-badge]

[Hutch][hutch-repo] Tools for working with the HDR UK Cohort Discovery Tool.

```
hutch-cohort-discovery/
├── app/
│   ├── 🐇 bunny/
│   ├── 🔄 Hutch.Relay/
│   └── 🔗 RQuestBridge/
└── lib/
    ├── 🎾 Hutch.Rackit/
    └── 📦 FiveSafes.Net/
```

## Applications

### 🐇 Bunny

| | | |
|-|-|-|
| ![Python][python-badge] | [![Bunny Docker Images][docker-badge]][bunny-containers] | [![Bunny Docs][docs-badge]][bunny-docs] |

An HDR UK Cohort Discovery Task Resolver.

Fetches and resolves Availability and Distribution Queries against a PostgreSQL OMOP-CDM database.

### 🔄 Relay

| | | |
|-|-|-|
| ![.NET][dotnet-badge] | [![Relay Docker Images][docker-badge]][relay-containers] | [![Relay Docs][docs-badge]][relay-docs] |

A Federated Proxy for the HDR UK Cohort Discovery Tool's Task API.

- Connects to an upstream Task API (e.g. the HDR UK Cohort Discovery Tool).
- Fetches tasks.
- Queues them for one or more downstream sub nodes (e.g. Bunny instances).
- Accepts task results from the downstream nodes.
- Submits aggregate results to the upstream Task API.

Implements a subset of the Task API for the downstream nodes to interact with.

### 🔗 RquestBridge

| | |
|-|-|
| ![.NET][dotnet-badge] | [![RQuest Bridge Docker Images][docker-badge]][bridge-containers]

An integration for the HDR UK Cohort Discovery Tool with the DARE UK TRE-FX architecture.

- Connects to an upstream Task API (e.g. the HDR UK Cohort Discovery Tool).
- Fetches tasks.
- Prepares a Five Safes RO-Crate to handle the task using the [`rquest-omop-worker` workflow][bridge-workflow] and Bunny CLI.
- Submits the prepared crate to a TRE-FX Submission Layer.
- Retrieves results from the TRE-FX Submission Layer and returns them to the upstream Task API.

## Libraries

### 🎾 RACKit

| | | |
|-|-|-|
| ![.NET][dotnet-badge] | [![RACKit NuGet package][nuget-badge]][rackit-packages] | [![RACKit Readme][readme-badge]][rackit-readme] |

RACKit is the RQuest API Client Kit, a .NET Library for interacting with the HDR UK Cohort Discovery Task API.

#### Samples

The `samples/` directory contains a sample application showcasing the use of RACKit to connect to a Task API.

### 📦 FiveSafes.Net

| |
|-|
| ![.NET][dotnet-badge]

A .NET library for working with the [Five Safes RO-Crate][5s-crate] profile.

[hutch-logo]: https://raw.githubusercontent.com/HDRUK/hutch/main/assets/Hutch%20splash%20bg.svg
[hutch-repo]: https://github.com/health-informatics-uon/hutch

[bunny-docs]: https://health-informatics-uon.github.io/hutch/bunny
[bunny-containers]: https://github.com/Health-Informatics-UoN/hutch-bunny/pkgs/container/hutch%2Fbunny

[relay-docs]: https://health-informatics-uon.github.io/hutch/relay
[relay-containers]: https://github.com/Health-Informatics-UoN/hutch-cohort-discovery/pkgs/container/hutch%2Frelay

[bridge-containers]: https://hub.docker.com/r/hutchstack/rquest-bridge
[bridge-workflow]: https://workflowhub.eu/workflows/471

[rackit-packages]: https://github.com/Health-Informatics-UoN/hutch-cohort-discovery/pkgs/nuget/Hutch.Rackit
[rackit-readme]: https://github.com/Health-Informatics-UoN/hutch-cohort-discovery/blob/main/lib/Hutch.Rackit/README.md

[5s-crate]: https://trefx.uk/5s-crate/

[license-badge]: https://img.shields.io/github/license/health-informatics-uon/hutch-cohort-discovery.svg
[dotnet-badge]: https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white
[python-badge]: https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white
[docker-badge]: https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white
[nuget-badge]: https://img.shields.io/badge/nuget-%23004880?style=for-the-badge&logo=nuget&logoColor=white
[docs-badge]: https://img.shields.io/badge/docs-black?style=for-the-badge&labelColor=%23222
[readme-badge]: https://img.shields.io/badge/readme-lightgrey?style=for-the-badge&labelColor=%23222
