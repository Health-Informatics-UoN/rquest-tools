# 🎾 Hutch.Rackit

RACKit is the Rquest API Client Kit, a .NET library for interacting with BC|RQuest's REST API, and part of the Hutch suite of tooling for federated analytics.

# Design notes

The intent for the library is to have self contained units that provide Rquest functionality (such as fetching query jobs, or submitting query responses), and then to compose those units in useful ways (such as providing a polling service that runs a configurable delegate when jobs are picked up). Users of the library can then use the high level services, or compose the low level pieces themselves in new and interesting ways.

Expected Services/Functionality initially:

- Api Client
  - Check a collection for availability queries
  - Check a collection for distribution queries
  - Submit a query response
- Polling Service
  - robust methods for performing query checks regularly, and triggering actions via configurable delegates
- Polling Hosted Service
  - .NET Hosted Service for running the above Polling Service as a long running background service in a .NET application.

# Getting started

It's early days, so currently work from source:

1. Have the latest .NET LTS SDK
1. Checkout the repo
1. Open the library .csproj

In future, expect:

- Sample apps showing usage
- GitHub packages from merges to `main` ("nightlies")
- GitHub releases
- Nuget.org packages for releases

# Documentation

## Task Api Client Reference

RACKit currently provides a limited client for interacting with the RQuest Task API. In future further API surface of RQuest may also be covered by this library.

### Configuration

The Task API Client requires some information about the instance of RQuest it is connecting to:

- BaseUrl - the url of RQuest, that API endpoints can be appended to
- CollectionId - a default Collection ID

### FetchQuery()

`FetchQuery()` will ask the API if there are any query jobs waiting of the specified query type, for the requested collection.

It requires some configuration information that can be:
- passed directly as arguments
- configured as default options on the service
- passed as overrides in an options object which will fall back to the service's defaults for any omitted values.

e.g.

```csharp


await FetchQuery<AvailabilityQuery>("https://example.com")
```

# Contributing

Hutch.Rackit is © 2024 University of Nottingham and available under the MIT License.

Please engage with GitHub issues and discussions to contribute. PRs to resolve open issues are welcome.
