# Task Api Client

RACKit currently provides a limited client for interacting with the RQuest Task API. In future further API surface of RQuest may also be covered by this library.

## Configuration

The Task API Client requires some information about the instance of RQuest it is connecting to:

- BaseUrl - the url of RQuest, that API endpoints can be appended to
- CollectionId - a default Collection ID
- Username - a valid API user for the target instance
- Password - the correct password for the above user

Client methods can be provided these details directly, or the service can be configured with default values, and they can be overridden if desirable when calling individual methods:

```csharp
// client is configured when constructed, optionally from DI
// but client defaults can be set later too:
client.Options = new ApiClientOptions {
  BaseUrl: "https://api.example.com"
  // ...
};

// Specify all details
await client.FetchQuery<AvailabilityQuery>(
  "https://override.example.com",
  "RQ-Collection-123",
  "user1",
  "password1");

// Use all defaults as specified when constructing the client / via DI
await client.FetchQuery<AvailabilityQuery>();

// Override some details - any omitted will fall back to the defaults
await client.FetchQuery<AvailabilityQuery>(new () {
  CollectionId = "RQ-Collection-XYZ"
});
```

## `FetchQuery()`

Fetch Query interacts with the Task API's `/nextjob` endpoint, to retrieve the next query job of a certain type for a given collection, if there is one.

The RACKit TaskApiClient currently supports the following query types:
- Availability
- Distribution

To specify the query type you want to fetch, specify the .NET Type for the query response model:

```csharp
await FetchQuery<AvailabilityQuery>();
await FetchQuery<DistributionQuery>();
```

FetchQuery returns null if there are no query jobs in the queue.

`FetchQuery()` only requires the `ApiClientOptions` arguments.
