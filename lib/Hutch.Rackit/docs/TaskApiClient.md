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
await client.FetchQueryAsync<AvailabilityQuery>(
  "https://override.example.com",
  "RQ-Collection-123",
  "user1",
  "password1");

// Use all defaults as specified when constructing the client / via DI
await client.FetchQueryAsync<AvailabilityQuery>();

// Override some details - any omitted will fall back to the defaults
await client.FetchQueryAsync<AvailabilityQuery>(new () {
  CollectionId = "RQ-Collection-XYZ"
});
```

## `FetchQueryAsync()`

Fetch Query interacts with the Task API's `/nextjob` endpoint, to retrieve the next query job of a certain type for a given collection, if there is one.

The RACKit TaskApiClient currently supports the following query types:
- Availability
- Distribution

To specify the query type you want to fetch, specify the .NET Type for the query response model:

```csharp
await FetchQueryAsync<AvailabilityQuery>();
await FetchQueryAsync<DistributionQuery>();
```

FetchQuery returns null if there are no query jobs in the queue.

`FetchQueryAsync()` only requires the `ApiClientOptions` arguments.

## `SubmitResultAsync()`

Submit Result interacts with the Task API's `/result` endpoint, to submit the result for a job from a given collection.

SubmitResult will throw an exception if the request was not accepted by the remote Task API.

`SubmitResultAsync()` requires a `jobId` and a `result` payload, and also accepts the `ApiClientOptions` arguments.

Task API Job Result payloads are consistent regardless of job type, however there are two approaches to results submission: with or without results files.

### Without results files

> [!NOTE]
> This is typical for Availability Queries.

Without results files, usage is quite straightforward:

```csharp
// Collection Id and Job Id can be retrieved from the job per the response to `FetchQueryAsync()`
await SubmitResultAsync(
  jobId: "a030666b-2aed-4657-a126-498355ce89c4",
  result: new Result() {
    Uuid = "a030666b-2aed-4657-a126-498355ce89c4",
    CollectionId = "RQ-Collection-XYZ",
    Status = "OK",
    Message = "Results",
    Results = new()
    {
      Count = 123,
      DatasetCount = 1,
      Files = [] // When results files are not used, this should be an empty array
    }
});
```

### With results files

> [!NOTE]
> This is typical for Distribution Queries.

With results files, guidance is yet to be provided, pending implementation of the ResultsFile API client methods.
