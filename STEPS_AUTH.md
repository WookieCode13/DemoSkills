# STEPS_AUTH - Cognito + JWT + .NET API (Learn One Step at a Time)

Goal: secure APIs with AWS Cognito tokens, then use tenant claim for schema routing.

Notes:
- Keep costs low.
- Move in order.
- Do not skip verification checkpoints.
- Architecture shift (current plan):
  - JWT is for authentication/identity.
  - DB is for tenant authorization/membership.

## Shared Security Modules (Current Repo Layout)

Created shared folders:
- `.NET`: `Shared/Security/dotnet/Shared.Security.Net`
- `Python`: `Shared/Security/py/shared_security_py`
- Spec doc: `Shared/Security/SecuritySpec.md`

Purpose:
- Keep auth/tenant logic reusable across APIs.
- Implement once, consume in EmployeeAPI/CompanyAPI/ReportAPI.


## Pre Steps: Check for HTTPS 443
I did not have 443 setup initially, so these are the setup notes to solve it (started from port 80 only).
[x] - check EC2 load balancer listener setup for 80 and/or 443 (if just 80 then continue setup)
[x] - AWS -> Certificate Manager (ACM), request/check cert
    - request : public
    - choose us-east-1 at top, in header.
    - request a cert (longranch.com and *.longranch.com)    
    - validation: DNS
    - RSA 2048
    - BLUE BUTTON: `Create records in Route 53`  (open ACM Cert, at top of domain list)
        - This creates CNAME validation records automatically in Route 53.
    - verify CNAME resolves 
    - wait and refresh ACM until status is `Issued`
[x] - Add HTTPS 443 listener to ALB
[x] - Add cert to 443 listener
[x] - Add redirect on 80 to 443
[x] - Add rules to 443
    - On 443 use forward rules only (no redirect-to-443 rules) to avoid redirect loops.


## Step 0: Decide First Scope (Do This Before Console Work)

Use this first version:
- Authentication provider: AWS Cognito User Pool
- API token type: Access Token (Bearer)
- Tenant claims (for quick context): `custom:tenant_short_code`, `custom:tenant_id`
- Tenant short code format: 10 uppercase alphanumeric (example: `DEMOSKILLS`)
- Groups (created):
  - `internal_admin` (precedence `100`)
  - `internal_support` (precedence `300`)
  - `external_user` (precedence `500`)
- Authorization model:
  - Use JWT claims for identity and coarse role/group.
  - Use DB mapping tables for tenant access decisions.

Done when:
- You agree on the above and keep it consistent in docs/code.

## Step 1: Create Cognito User Pool

AWS Console -> Cognito -> User Pools -> Create user pool

Choose:
- Define application: Single-page application (SPA)
- Name: `SPA-DemoSkills-App`
- Sign-in option: Email
- Development platform: JavaScript
- (optional) Password policy: Default
- (optional) MFA: Disabled (for now)
- Self sign-up: Disabled
- (optional) Advanced security: Disabled (for now)

Record:
- `Region`
- `UserPoolId`

Done when:
- User pool exists and you saved Region + UserPoolId.

## Step 2: Create User Pool App Client
Check Settings: User Pool -> App clients -> select app client -> Login pages tab

Inside your user pool:
- Create app client
- Client type: Public client (no client secret)
- Enable OAuth 2.0
- Allowed OAuth flows: Authorization Code + Refresh Token
- Allowed scopes: `openid`, `email`, `profile`

Add callback URL (for Swagger later):
- `https://localhost:5001/swagger/oauth2-redirect.html`

Record:
- `AppClientId`

Done when:
- App client exists and you saved AppClientId.

## Step 3: Create Cognito Domain
Check setup path: User Pools -> Branding -> Domain -> Cognito domain (not custom)

Inside user pool:
- Branding -> Domain -> Create domain
- Example prefix: `demoskills-auth`

Record:
- Hosted UI domain URL `https://us-east-1dcbgcjele.auth.us-east-1.amazoncognito.com`
- Token endpoint:
  - `https://us-east-1dcbgcjele.auth.us-east-1.amazoncognito.com/oauth2/token`
- Authorize endpoint:
  - `https://us-east-1dcbgcjele.auth.us-east-1.amazoncognito.com/oauth2/authorize`
- Logout endpoint:
  - `https://us-east-1dcbgcjele.auth.us-east-1.amazoncognito.com/logout`

Done when:
- Domain is active and both URLs are known.

## Step 4: Add Tenant Attribute

Primary UI path (new console):
- User pool -> Sign-up
- Find Custom attributes (or User attributes section)
- Add custom attribute
- Name: `tenant_short_code` and  `tenant_id` uuid
- Type: String
- Mutable: Yes

If the UI does not show add custom attribute, use AWS CLI:
```bash
aws cognito-idp add-custom-attributes \
  --user-pool-id <USER_POOL_ID> \
  --custom-attributes Name=tenant_short_code,AttributeDataType=String,Mutable=true,StringAttributeConstraints={MinLength=10,MaxLength=10}
```
```bash
aws cognito-idp add-custom-attributes \
  --user-pool-id <USER_POOL_ID> \
  --custom-attributes Name=tenant_id,AttributeDataType=String,Mutable=true,StringAttributeConstraints={MinLength=36,MaxLength=36}
```

Tenant rule for this project:
- Must be 10 chars, uppercase A-Z and 0-9.
- Example: `DEMOSKILLS`
- `tenant_id` should be UUID text (36 chars).

Done when:
- `custom:tenant_short_code` and `custom:tenant_id` exist in user attributes.

## Step 5: Create Test User

Inside user pool:
- Users -> Create user
- Set email + temporary password
- Set `custom:tenant_short_code = DEMOSKILLS` (if visible in user attributes)
- Set `custom:tenant_id = <uuid>` (if visible in user attributes)
- If custom attributes are not visible, confirm Step 4 actually saved first.

https://<your-cognito-domain>/oauth2/authorize?response_type=code&client_id=4qbcsrvqhohbi8bufsqu95jlhd&redirect_uri=https%3A%2F%2Flongranch.com%2F&scope=openid

First login:
- Complete password reset so account is permanent.

Done when:
- Test user can sign in successfully.

## Step 6: Get Token Manually (Learning Check)

Use Cognito Hosted UI Authorization Code flow first (recommended).

Why:
- It matches production better than password-grant style testing.

Run this:
- Open this URL in browser (replace values if needed):
  - `https://<your-cognito-domain>/oauth2/authorize?response_type=code&client_id=<AppClientId>&redirect_uri=https%3A%2F%2Flongranch.com%2F&scope=openid`
- Example using this project values:
  - `https://us-east-1dcbgcjele.auth.us-east-1.amazoncognito.com/oauth2/authorize?response_type=code&client_id=4qbcsrvqhohbi8bufsqu95jlhd&redirect_uri=https%3A%2F%2Flongranch.com%2F&scope=openid`
- Sign in with your test user.
- If prompted, complete password change.

Expected result:
- Browser redirects to `https://longranch.com/?code=...`
- The `code` query parameter means auth code flow worked.

If it fails with `Invalid request`:
- Verify app client callback URL includes exactly `https://longranch.com/`
- Verify Authorization code flow is enabled on app client Login pages
- Verify `openid` scope is enabled
- Verify the client ID belongs to this same user pool/domain

Done when:
- You can sign in and receive `code=...` in the callback URL.

Check:
- Keep the code for the next step (token exchange to inspect claims).

## Step 7: Configure .NET API JWT Validation

In API config:
- Authority: `https://cognito-idp.<region>.amazonaws.com/<UserPoolId>`
- Audience: `<AppClientId>`

### 7.1 Add config values
In `appsettings.Development.json` (or env vars):
```json
{
  "Jwt": {
    "Authority": "https://cognito-idp.us-east-1.amazonaws.com/<UserPoolId>",
    "Audience": "<AppClientId>"
  }
}
```

Env var equivalents:
- `Jwt__Authority`
- `Jwt__Audience`

### 7.2 Wire auth in `Program.cs`
Add JWT auth services:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
    });

builder.Services.AddAuthorization();
```

Add middleware in pipeline order:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### 7.3 Protect endpoints
Add `[Authorize]` to controller/actions you want secured.
Keep health endpoint public for now if desired.

### 7.4 Quick verification
- Call secured endpoint without token -> `401`
- Call secured endpoint with invalid token -> `401`
- Call secured endpoint with valid Cognito token -> success (`200/201/204`)

Done when:
- API rejects missing/invalid token.
- API accepts valid Cognito token.

## Step 7.5: Wire Shared .NET Security into EmployeeAPI (Next Step)

Goal:
- Start using `Shared.Security.Net` from `EmployeeAPI` first.

Tasks:
- Add project reference from `apis/EmployeeAPI/EmployeeAPI.csproj` to:
  - `Shared/Security/dotnet/Shared.Security.Net/Shared.Security.Net.csproj`
- Move auth wiring into shared extension methods (from `Shared.Security.Net`).
- Add one middleware/dependency that resolves principal + tenant context.
- Keep health endpoint public, secure one employee endpoint first.

Verification:
- No token -> `401`
- Valid token, wrong group -> `403`
- Valid token + allowed group -> success

Done when:
- EmployeeAPI uses shared security package and passes basic auth checks.

## Step 8: Connect Swagger to Cognito

In Swagger/OpenAPI config:
- Security scheme: OAuth2 Authorization Code
- Authorization URL: `https://<domain>.auth.<region>.amazoncognito.com/oauth2/authorize`
- Token URL: `https://<domain>.auth.<region>.amazoncognito.com/oauth2/token`
- Scope: `openid` (add more later if needed)

Done when:
- Swagger Authorize button opens Cognito login.
- Swagger sends bearer token on API calls.

## Step 9: Tenant to Schema Mapping (After Auth Works)

Read tenant context from token claims (not request header).

Validation:
- Must match `^[A-Z0-9]{10}$`
- Must map to known/allowed tenant schema

Schema example:
- `"DEMOSKILLS".employee`

Security rule:
- JWT identity is authoritative; DB membership is authorization source of truth.

Done when:
- Employee writes/reads route correctly by tenant claim.

## Step 9.5: Authorization Shift (JWT + DB)

Target model:
- JWT supplies identity: `sub`, `email`, groups, tenant context hints.
- DB supplies authorization:
  - Which tenants user can access
  - Role per tenant (if needed)

Recommended global/shared tables (not per-tenant schema):
- `auth_user` (maps Cognito `sub`)
- `tenant`
- `user_tenant_membership`

Why:
- Avoid manual per-tenant Cognito group management at scale.
- Keep internal/external separation via groups, and tenant scoping via DB.

## Step 10: Add Authorization Roles (Next Iteration)

Recommended:
- Use Cognito groups for role checks:
  - `internal_admin`
  - `internal_support`
  - `external_user`
- Enforce policies in API per endpoint

Done when:
- Unauthorized role gets `403`.
- Allowed role succeeds.

## Step 11: Production Hardening

- Enforce HTTPS everywhere
- Tight CORS rules
- Short access token lifetime
- Refresh token strategy
- Log auth failures and tenant mismatches
- Add alarms/monitoring for repeated auth errors

Done when:
- Security baseline is documented and active in deployed environments.
