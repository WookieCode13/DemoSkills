# STEPS_AUTH - Cognito + JWT + .NET API (Learn One Step at a Time)

Goal: secure APIs with AWS Cognito tokens, then use tenant claim for schema routing.

Notes:
- Keep costs low.
- Move in order.
- Do not skip verification checkpoints.


## Pre Steps: Check for HTTPS 443
I do not have 443 setup, create notes hear hwo to solve. Just port 80.
[x] - check for EC2 load balance setup 80 and/or 443 (if just 80 then continue set up)
[ ] - AWS → Certificate Manager (ACM), check for cert. (previous have failed)
    - request : public
    - choose us-east-1 at top, in header.
    - request a cert (longranch.com and *.longranch.com)    
    - validation: DNS
    - RSA 2048
    - BLUE BUTTON: `Create Records in Route 53`  (open ACM Cert, at top of domain list)
        - Add the CNAME records immediately in your DNS host.
    - verify CNAME resolves 
    - wait and refresh ACM until status is - `Issued`
[ ] - Add HTTPS 443 listener to ALB
[ ] - Add Cert to Listener
[ ] - Add Redirect on 80 to 443
[ ] - Add rules to 443


## Step 0: Decide First Scope (Do This Before Console Work)

Use this first version:
- Authentication provider: AWS Cognito User Pool
- API token type: Access Token (Bearer)
- Tenant model: `custom:tenant` claim
- Tenant format: 10 uppercase alphanumeric (example: `DEMOSKILLS`)
- Roles: start simple now, add groups later

Done when:
- You agree on the above and keep it consistent in docs/code.

## Step 1: Create Cognito User Pool

AWS Console -> Cognito -> User Pools -> Create user pool

Choose:
- Define Application: Single Page Spa
- Name: `SPA-DemoSkills-App`
- Sign-in option: Email
- development platform : JS Javascript (usable on other platforms)
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

Inside user pool:
- Domain -> Create domain
- Example prefix: `demoskills-auth`

Record:
- Hosted UI domain URL
- Token endpoint:
  - `https://<domain>.auth.<region>.amazoncognito.com/oauth2/token`
- Authorize endpoint:
  - `https://<domain>.auth.<region>.amazoncognito.com/oauth2/authorize`

Done when:
- Domain is active and both URLs are known.

## Step 4: Add Tenant Attribute

Inside user pool:
- Attributes -> Add custom attribute
- Name: `tenant`
- Type: String
- Mutable: Yes

Tenant rule for this project:
- Must be 10 chars, uppercase A-Z and 0-9.
- Example: `DEMOSKILLS`

Done when:
- `custom:tenant` exists in user attributes.

## Step 5: Create Test User

Inside user pool:
- Users -> Create user
- Set email + temporary password
- Set `custom:tenant = DEMOSKILLS`

First login:
- Complete password reset so account is permanent.

Done when:
- Test user can sign in successfully.

## Step 6: Get Token Manually (Learning Check)

Use Hosted UI Authorization Code flow first (recommended).

Why:
- It matches production better than password-grant style testing.

Done when:
- You can obtain a valid JWT token and inspect claims.

Check:
- Token includes expected claims.
- Tenant value is present where you plan to read it.

## Step 7: Configure .NET API JWT Validation

In API config:
- Authority: `https://cognito-idp.<region>.amazonaws.com/<UserPoolId>`
- Audience: `<AppClientId>`

Enable:
- Authentication middleware
- Authorization middleware

Done when:
- API rejects missing/invalid token.
- API accepts valid Cognito token.

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

Read tenant from token claims (not request header).

Validation:
- Must match `^[A-Z0-9]{10}$`
- Must map to known/allowed tenant schema

Schema example:
- `"DEMOSKILLS".employee`

Security rule:
- JWT tenant claim is authoritative.

Done when:
- Employee writes/reads route correctly by tenant claim.

## Step 10: Add Authorization Roles (Next Iteration)

Recommended:
- Use Cognito groups for role checks (`admin`, `user`, etc.)
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

