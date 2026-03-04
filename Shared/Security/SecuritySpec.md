# DemoSkills Security Spec

This document defines the shared JWT authentication model for both .NET and Python APIs.

## Common Contract

- Token type for API access: `access_token`
- Authority/issuer config key: `Jwt__Authority`
- Client id config key: `Jwt__ClientId`
- Required claim checks:
  - `token_use == "access"`
  - `client_id == Jwt__ClientId`
- Signature algorithm: `RS256`
- JWKS endpoint: `{Jwt__Authority}/.well-known/jwks.json`

## .NET JWT Flow

```mermaid
flowchart TD
    A[Client sends Bearer token] --> B[ASP.NET pipeline]
    B --> C[UseAuthentication]
    C --> D[Shared.Security.Net AddDemoSkillsJwtAuth]
    D --> E[JwtBearer validates signature + issuer]
    E --> F[Custom claim checks token_use and client_id]
    F -->|valid| G[HttpContext.User populated]
    F -->|invalid| H[401 invalid_token]
    G --> I[UseAuthorization + endpoint execution]
```

## Python JWT Flow

```mermaid
flowchart TD
    A[Client sends Bearer token] --> B[FastAPI route dependency]
    B --> C[get_current_principal]
    C --> D[load settings from Jwt__Authority and Jwt__ClientId]
    D --> E[Read kid from JWT header]
    E --> F[Fetch JWKS from authority]
    F --> G[Validate signature + issuer]
    G --> H[Check token_use and client_id]
    H -->|valid| I[Principal returned to route]
    H -->|invalid| J[401 invalid_token]
```

## Operational Notes

- Health endpoints can remain anonymous while business endpoints require auth.
- Swagger/OpenAPI should use Bearer auth in every API.
- Same Cognito token is accepted by all APIs when `Jwt__Authority` and `Jwt__ClientId` match.
- If using shared code in Docker builds, use repo root build context so shared folders are available.
