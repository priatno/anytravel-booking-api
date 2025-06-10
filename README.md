# AnyTravel Booking API

Handles booking creation and payment status updates for AnyTravel's flight booking flow.

## Running locally

```bash
dotnet restore
dotnet run --project src/BookingApi
```

## Configuration

Connection string is built from environment variables at startup (see `Program.cs`).

## Deployment

Deployed to AWS Elastic Beanstalk (.NET Core on Linux platform). See `.ebextensions/` for environment config and `buildspec.yml` for the CodeBuild/CodePipeline steps.
