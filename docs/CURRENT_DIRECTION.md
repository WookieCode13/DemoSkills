# Current Direction

This note captures the current simplification direction for the project so the repo reflects the intended path forward.

## Goals

- Prioritize working backend and UI flows over ideal architecture.
- Keep enough AWS surface area to demo deployment, messaging, and background processing.
- Reduce stack and service sprawl where it is slowing down delivery.

## Current Leaning

### 1. Reuse this repo

- Keep working in `DemoSkills` unless the name becomes a real portfolio problem.
- Renaming later is cheaper than rebuilding the app, AWS wiring, and Harness setup now.

### 2. Consolidate to .NET where practical

- Converting Python APIs to .NET should reduce maintenance friction.
- Python can still be used later in a separate project if needed for learning or experimentation.

### 3. Stay single-tenant for now

- Do not let schema-per-tenant or multitenant design block delivery.
- Keep `_auth` separated as needed, but keep app tables on the normal EF path.
- Company can remain business data without implementing true tenant isolation yet.

### 4. Reduce API sprawl

Current runtime shape on the branch:

- `Admin API`: company + employee
- `Payroll API`: pay + tax
- `Report API`: separate for heavier processing and async/report demos

This is what the current Docker/runtime wiring reflects today, even though some legacy service directories still exist in the repo.

### 5. Use reports for queue/lambda demos

- Report generation is the best fit for async/background work.
- Likely flow:
  - request creates a report job
  - queue/background worker generates report output
  - status/result endpoint returns progress or completion

## What Should Not Block Progress

- Perfect naming
- Full multitenant design
- Excessive architecture changes in one branch
- Keeping legacy env var names temporarily

## Likely Next Decisions

1. Confirm the target service shape:
   - keep multiple APIs
   - consolidate to 3 APIs
   - or go more monolithic
2. Finish converting `ReportAPI` to .NET and remove remaining Python assumptions.
3. Decide whether to delete or archive the legacy `CompanyAPI` code now that company routes are served from `EmployeeAPI`.
4. Decide whether to delete or archive the legacy `TaxCalculatorAPI` assumptions now that tax routes are served from `PayAPI`.
5. Start building actual UI and backend workflows instead of more infrastructure churn.

## Practical Reminder

- Make major simplification changes in separate branches.
- Keep a short decision log when architecture direction changes.
- Optimize for momentum and demonstrable app behavior.
