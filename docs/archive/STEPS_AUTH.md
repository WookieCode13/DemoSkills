# STEPS_AUTH - Cognito + JWT + .NET API (Learn One Step at a Time)

Goal: secure APIs with AWS Cognito tokens, then use tenant claim for schema routing.

Notes:
- Keep costs low.
- Move in order.
- Do not skip verification checkpoints.
- Architecture shift (current plan):
  - JWT is for authentication/identity.
  - DB is for tenant authorization/membership.

  Auth
  ├─ IAuthorizationService
  ├─ AuthorizationService
  ├─ IUserAuthContextProvider
  ├─ IUserAuthRepository
  ├─ UserAuthContext
  └─ UserPermission

  Request
    ↓
  JWT Auth   > get token from cognito
    ↓
  IUserAuthContextProvider > GetUserAuthContextAsync from http
    ↓
  IUserAuthRepository (DB lookup) > GetByCognitoSubAsync | permissions
    ↓
  UserAuthContext  < returned from Repos above
    ↓
  AuthorizationService  >  HasPermissionAsync
    ↓
  Permission decision

Request
  ↓
[Authorize Policy]
  ↓
PermissionAuthorizationHandler
  ↓
IPermissionService
  ↓
UserAuthContextProvider
  ↓
UserAuthRepository

## Shared Security Modules (Current Repo Layout)

Created shared folders:
- `.NET`: `Shared/Security/dotnet/Shared.Security.Net`
- `Python`: `Shared/Security/py/shared_security_py`
- Spec doc: `Shared/Security/SecuritySpec.md`

Purpose:
- Keep auth/tenant logic reusable across APIs.
- Implement once, consume in EmployeeAPI/CompanyAPI/ReportAPI/PayAPI/TaxCalculatorAPI.

Current implementation notes:
- `.NET` shared auth is wired through `Shared.Security.Net.Auth.AddDemoSkillsJwtAuth(...)`.
- `EmployeeAPI`, `PayAPI`, and `TaxCalculatorAPI` currently use the shared `.NET` JWT validation.
- `CompanyAPI` and `ReportAPI` currently use `shared_security_py.get_current_principal`.
- Docker Compose now expects `DEMOSKILLS_JWT_AUTHORITY` and `DEMOSKILLS_JWT_CLIENT_ID` for all APIs.
- Python API containers use `PYTHONPATH=/app:/app/shared`; keep shared import bootstrap logic container-safe.


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
  - `external_employee` (precedence `700`)
- Authorization model:
  - Use JWT claims for identity and coarse role/group.
  - Use DB mapping tables for tenant access decisions.

Done when:
- You agree on the above and keep it consistent in docs/code.

## Step 0.5: Role Design Working Section

Use this section for the role model you are designing next.

Keep these base roles:
- `100`
- `300`
- `500`
- `700`

Suggested structure to paste in below:
- Role name
- Base level (`100` / `300` / `500` / `700`)
- Intended users
- Allowed APIs/endpoints
- Read vs write permissions
- Tenant-scoped or global
- Notes/questions

Role design draft:
- `internal_admin` (`100`):
- `internal_support` (`300`):
- `external_user` (`500`):
- `external_employee` (`700`):

Open questions:
- Which claim will carry role information: Cognito group, custom claim, or both?
- Will `100/300/500/700` map 1:1 to Cognito groups, or will groups expand later into named roles?
- Which endpoints stay public (`/health`, Swagger/docs), and which require auth by default?

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
- ClientId: `<AppClientId>`

### 7.1 Add config values
In `appsettings.Development.json` (or env vars):
```json
{
  "Jwt": {
    "Authority": "https://cognito-idp.us-east-1.amazonaws.com/<UserPoolId>",
    "ClientId": "<AppClientId>"
  }
}
```

Env var equivalents:
- `Jwt__Authority`
- `Jwt__ClientId`

Current repo note:
- For Docker Compose, these come from:
  - `DEMOSKILLS_JWT_AUTHORITY`
  - `DEMOSKILLS_JWT_CLIENT_ID`
- PayAPI and TaxCalculatorAPI now require these at startup just like EmployeeAPI.

### 7.2 Wire auth in `Program.cs`
Add JWT auth services:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        var expectedClientId = builder.Configuration["Jwt:ClientId"];
        options.TokenValidationParameters.ValidateAudience = false;
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
                var clientId = context.Principal?.FindFirst("client_id")?.Value;
                if (tokenUse != "access" || clientId != expectedClientId)
                {
                    context.Fail("Invalid Cognito access token.");
                }
                return Task.CompletedTask;
            }
        };
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

Current status:
- EmployeeAPI: shared `.NET` auth wired
- PayAPI: shared `.NET` auth wired
- TaxCalculatorAPI: shared `.NET` auth wired
- CompanyAPI: shared Python auth wired
- ReportAPI: shared Python auth wired

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
- Keep the base role levels:
  - `100`
  - `300`
  - `500`
  - `700`
- Keep the current group names:
  - `internal_admin`
  - `internal_support`
  - `external_user`
  - `external_employee`
- Decide whether those are represented directly as Cognito groups or mapped from named groups.
- Enforce policies in API per endpoint

Suggested rollout:
- Start with read-only `GET` protection in each API.
- Then add write checks for `POST`/`PATCH`/`DELETE`.
- Keep policy names aligned across `.NET` and Python implementations.

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


#  ---- my thoughts  ----

## Authorization Design (DemoSkills – Initial Version)

Goal: implement a **middle-complexity authorization model** that is not over-engineered but supports:
- internal staff access
- external company users
- switching companies
- CRUD permissions by role
- future employee self-service access

Authentication is handled by **Cognito JWT tokens**.  
Authorization logic is handled by **application code + database tables**.

---

# Authentication

JWT token provides identity and coarse trust level.

Claims used:

- `sub` → Cognito user identifier
- `email`
- `user_level` (optional convenience)

JWT is validated by each API (.NET and Python shared security libraries).

After validation the claims become the **principal**.

---

# User Privilege Levels

Lower number = higher privilege.

| Level | Description |
|------|-------------|
| 100 | super_admin |
| 300 | internal customer roles (cust_manager, cust_service) |
| 500 | external company users (owner, payroll_admin) |
| 700 | future employee self-service users |

Level is used for **broad authorization rules**, not fine permissions.

Example rules:

- `100` → full system access
- `300` → internal users, may access many companies
- `500` → external users, limited to assigned companies
- `700` → employee self-service only

---

# Company Switching

Users may access multiple companies.

Each request selects the **active company**.

Current mechanism:

```
X-Company: COMPANY_CODE
```

Future option:
```
/companies/{companyCode}/...
```

## My Thoughts Reworked Into A V1 Model

Goal:
- Keep authentication in Cognito.
- Keep authorization in Postgres.
- Use a global base role for coarse system blocking.
- Use company-scoped roles for detailed business permissions.

This gives two layers:
- Layer 1: base role decides broad access to systems/modules
- Layer 2: company role decides what the user can do inside that company

## Authorization Design V2

### Core Idea

Use three layers:
- base role on the user for coarse system access
- optional all-companies role on the user for global company-scope permissions
- company-specific role on membership for per-company permissions

Interpretation:
- base role answers: "Should this user be in this system at all?"
- all-companies role answers: "What can this user do across all companies?"
- company role answers: "What can this user do for this specific company?"

### Base Roles

Global base roles:
- `100` = `internal_admin`
- `300` = `internal_support`
- `500` = `external_user`
- `700` = `external_employee`

Purpose of base role:
- coarse system blocking
- high-level persona for UI shaping
- top-level security boundary before detailed permission checks

Examples:
- `700 external_employee` can be blocked from payroll entirely
- `500 external_user` may access customer-facing systems only
- `300 internal_support` may access support/admin systems
- `100 internal_admin` may access all systems

The future SPA can also use base role for:
- app/module visibility
- nav layout
- dashboard widget visibility

Important:
- frontend can shape UI from base role
- backend must still enforce all access rules

### Recommended V2 Tables

Schema naming note:
- use one shared internal auth schema named `_auth`
- example tables:
  - `_auth.app_user`
  - `_auth.role`
  - `_auth.permission`
  - `_auth.user_company_access`
  - `_auth.role_permission`
- reserve leading `_` for internal/system-owned schemas
- tenant/company-derived schemas should not use `_`

#### _auth.app_user

Represents authenticated identities from Cognito.

Fields:
- `app_user_id`
- `cognito_sub`
- `email`
- `base_role_level`
- `global_role_code`
- `is_active`
- `created_utc`
- `updated_utc`

Notes:
- `base_role_level` is global and coarse
- `global_role_code` is nullable
- `NULL` means the user has no global any-company role
- if present, `global_role_code` applies across any company

Examples:
- internal admin:
  - `base_role_level = 100`
  - `global_role_code = super_admin`
- support manager:
  - `base_role_level = 300`
  - `global_role_code = support_manager`
- new hire support:
  - `base_role_level = 300`
  - `global_role_code = NULL`

#### _auth.user_company_access

Represents company membership for a user.

Fields:
- `user_company_access_id`
- `app_user_id`
- `company_id`
- `company_role_code`
- `is_active`
- `created_utc`
- `updated_utc`

Notes:
- this stores company-specific roles
- use `company_id` as the source of truth, not `company_short_code`
- external users usually require rows here
- internal users can also use rows here for company-specific elevated access

Examples:
- customer service rep assigned to 12 client companies
- external payroll admin for one company
- owner/admin for a single tenant

#### _auth.role

Defines role identities separately from permissions.

Fields:
- `role_code`
- `role_name`
- `description`
- `is_active`
- `created_utc`
- `updated_utc`

Notes:
- users are assigned roles
- roles expand into many permission rows
- keep role definitions separate from permission definitions

#### _auth.permission

Defines atomic permissions.

Fields:
- `permission_code`
- `permission_name`
- `description`
- `system_code`
- `resource_code`
- `can_create`
- `can_read`
- `can_update`
- `can_delete`
- `is_active`
- `created_utc`
- `updated_utc`

Notes:
- prefer `resource_code` over raw `endpoint_code`
- resource/action is easier to maintain than route-by-route permissions
- this table defines the actual allowed operations
- examples:
  - `pay-view`
  - `pay-save`
  - `pay-delete`
  - `company-read`
  - `company-update`

#### _auth.role_permission

Maps roles to permissions.

Fields:
- `role_code`
- `permission_code`
- `created_utc`

Notes:
- `role_code` should reference `_auth.role.role_code`
- `permission_code` should reference `_auth.permission.permission_code`
- both `global_role_code` and `user_company_access.company_role_code` resolve into this same permission model

Example:

| role | permission |
|-----|------|
| support_manager | company-read |
| support_manager | company-update |
| payroll_admin | pay-view |
| payroll_admin | pay-save |
| employee | employee-profile-read |

### Recommended Authorization Flow

For each request:

1. Validate JWT
2. Build principal from claims
3. Load `_auth.app_user` by `cognito_sub`
4. Check `base_role_level` against the target system
5. Determine active company
6. Resolve effective role:
   - if a company-specific role exists in `_auth.user_company_access`, use it
   - else if `global_role_code` exists, use it
   - else deny
7. Load role metadata from `_auth.role` if needed
8. Resolve role permissions through `_auth.role_permission`
9. Evaluate the needed permission from `_auth.permission`
10. Set Postgres schema for the request
11. Execute the request

This keeps one shared authorization path for internal and external users.

### How To Determine Active Company

This should be explicit.

Recommended order:
1. trusted company selector from request/header/path
2. resolve company by short code
3. get `company_id`
4. resolve effective role for that company
5. then set schema

Future improvement:
- allow a default company for SPA convenience

### Example Scenarios

Example 1: internal admin
- `base_role_level = 100`
- `global_role_code = super_admin`
- request goes to payroll API for any company
- system access is allowed by base role
- effective role resolves to `super_admin`
- request succeeds if `super_admin` allows that action

Example 2: new hire support user
- `base_role_level = 300`
- `global_role_code = NULL`
- no `_auth.user_company_access` rows yet
- user can enter support-facing systems only if allowed by base role
- user cannot act on any company data yet because no effective role resolves

Example 3: customer service rep
- `base_role_level = 300`
- `global_role_code = customer_service_read`
- user also has `_auth.user_company_access` rows for assigned clients with role `customer_service`
- result:
  - broad read/search can come from `customer_service_read`
  - update permissions for assigned clients come from `_auth.user_company_access.company_role_code`

Example 4: external payroll admin
- `base_role_level = 500`
- `global_role_code = NULL`
- membership exists for `DEMOSKILLS`
- membership role is `payroll_admin`
- request to pay API for `DEMOSKILLS` succeeds
- request to a different company fails

Example 5: external employee self-service
- `base_role_level = 700`
- request goes to payroll admin endpoint
- denied before role lookup
- reason: base role blocks the payroll system

### Why V2 Seems Better

It keeps the strong parts of v1:
- Cognito handles identity
- DB handles authorization
- same shared rules can apply in `.NET` and Python

It also models your newer cases better:
- support managers with global cross-company roles
- customer service with broad read but assigned-company updates
ation to - new hires with a support base role but no company access yet
- future SPA shaping from base role without trusting the SPA for enforcement

### V2 Recommendation

Build this first:
- global `base_role_level` on `_auth.app_user`
- nullable `global_role_code` on `_auth.app_user`
- role definitions in `_auth.role`
- permission definitions in `_auth.permission`
- company-specific `company_role_code` on `_auth.user_company_access`
- role-to-permission mappings in `_auth.role_permission`

Do not build yet:
- custom per-user permission overrides
- route-by-route metadata tables
- central auth microservice

Initial version should stay simple, predictable, and easy to debug.

## Friday Next Steps

- Remove the temporary auth test endpoint once endpoint-level permission checks are in normal routes.
- Move endpoint authorization from proof-of-concept calls into real API endpoints.
- Keep `/health` public unless a specific API needs it secured.

### Shared `.NET` auth

- Keep `IUserAuthRepository`, `IUserAuthContextProvider`, `UserAuthContext`, and `AuthorizationService` as the common path.
- Add permission enforcement helpers/patterns that are easy to reuse in controllers and services.
- Decide whether endpoint checks should live:
  - directly in controllers first, or
  - behind shared policies/attributes next.
- Keep the `sub` / `ClaimTypes.NameIdentifier` fallback until JWT claim mapping is standardized.

### Shared Python auth

- Finish `Shared/Security/py/shared_security_py` so Python APIs use the same concepts as `.NET`:
  - token validation
  - current principal resolution
  - auth context loading by Cognito `sub`
  - permission evaluation
- Match naming across stacks:
  - `UserAuthContext`
  - repository/provider concepts
  - permission codes
- Do not invent a separate Python auth model; mirror the `.NET` flow.

### API rollout

- EmployeeAPI:
  - replace temp test usage with real endpoint permission checks
  - keep repository-backed auth lookup as the source of truth
- CompanyAPI:
  - move onto shared Python auth context + permission checks
- ReportAPI:
  - move onto shared Python auth context + permission checks
- PayAPI:
  - add repository-backed authorization checks beyond JWT validation
- TaxCalculatorAPI:
  - add repository-backed authorization checks beyond JWT validation

### Permission rollout

- Start with read permissions on protected GET endpoints.
- Then add create/update/delete checks.
- Keep permission names stable across APIs, for example:
  - `employee-profile-read`
  - `employee-profile-update`
  - `company-read`
  - `company-update`
  - `pay-read`
  - `pay-update`
  - `report-read`
  - `tax-run`

### Frontend after backend auth is stable

- Build login through Cognito Hosted UI or SPA auth flow.
- Store and send access token to APIs.
- Use base role and permission results to shape navigation, but keep backend enforcement authoritative.
- Add company selection UX once tenant/company authorization is working cleanly.

### Immediate implementation order

- 1. Wire shared auth checks into real EmployeeAPI endpoints.
- 2. Finish shared Python auth package shape.
- 3. Move CompanyAPI and ReportAPI to shared Python auth.
- 4. Add repository-backed authorization checks to PayAPI and TaxCalculatorAPI.
- 5. Remove temporary auth test endpoint.
- 6. Start frontend login flow only after API auth behavior is stable.

