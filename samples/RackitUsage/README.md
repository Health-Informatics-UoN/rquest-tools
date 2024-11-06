# Rackit Usage Samples

This is a single app that showcases some ddifferent ways of bootstrapping an app and using some RACKit functionality.

## Usage

1. Modify configuration at the top of Program.cs as desired
     - **[Required]** Specify connection details for a Task API
     - _[Optional]_ Change the default Sample behaviour
2. Run the app `dotnet run`
    - _[Optional]_ Specify the Sample behaviour as an argument e.g. `dotnet run -- simplepolling`

## Program.cs structure

The `Program.cs` for this app does 3 things:

1. Allows you, the sample runner, to configure it by changing hardcoded options at the top of the file.
2. Attempts to read the selected sample behaviour from the command line
3. 

## Sample Behaviours
