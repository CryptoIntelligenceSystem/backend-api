# Backend API tests

The test project covers the API without requiring a running ClickHouse instance.
It uses fake repositories and a fake ClickHouse client so tests are deterministic.

Run from `backend-api`:

```powershell
dotnet test .\tests\CryptoIntelligence.Api.Tests\CryptoIntelligence.Api.Tests.csproj
```

Covered areas:

- Query validation and limits.
- Service delegation and supported indicator/signal types.
- Repository SQL/table selection and aliases.
- Health and market controllers.
- HTTP error mapping middleware.
- HTTP integration through ASP.NET routing and model binding.
- Swagger JSON exposure.
