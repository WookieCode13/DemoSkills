# Change Log / Decision Notes

## 2026-03-26
- Decision direction: simplify the app to move faster on backend/UI/AWS features.
- Current leaning is away from true multitenant schema routing in EF Core.
- Reason: auth is in decent shape, but schema-per-tenant is adding friction and slowing feature work.
- Keep `_auth` schema as shared auth/authorization infrastructure.
- Revert app/business tables back toward normal public-schema usage for now.
- Continue using company as business data, but not as a hard tenant-isolation boundary yet.

## 2026-03-25
- Added shared auth reseed migration update and payroll/report schema setup migration.
- Fixed auth reseed migration so roles are upserted instead of deleted, avoiding FK failures from `_auth.app_user` and `_auth.user_company_access`.
- Added tax and report code pattern notes into the payroll/report migration for future reference.

## 2026-03-20
- Shared auth aligned across `.NET` and Python using the global-role path:
  - JWT validates identity
  - app user is resolved by Cognito `sub`
  - permissions come from `_auth.role_permission` + `_auth.permission`
- `CompanyAPI` and `ReportAPI` were updated to use shared Python auth context and permission checks.
- `PayAPI` and `TaxCalculatorAPI` were updated to use shared `.NET` auth services and policies.

## 2026-03-17
- Auth matrix expanded and reformatted.
- New seeded permissions included `employee-self-read` and `employee-self-update`.
- Payroll/report direction started to shift toward:
  - `pay_entry`
  - `sandbox_pay_entry`
  - `pay_check`
  - `report_data`
- Shared `public.tax` and `public.report` tables were introduced as metadata/reference tables.

## Working Direction
- Main goal: get to working backend workflows, UI integration, and AWS demo patterns faster.
- Prefer fewer architectural experiments until app behavior is in place.
- Good candidate future shape:
  - admin API: company + employee
  - payroll API: pay + tax
  - report API: report generation/async work
