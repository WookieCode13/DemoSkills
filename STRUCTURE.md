# Project Structure

## System Overview
```mermaid
graph TD
    A[Client] --> B[ALB (API Gateway future)]
    B --> C1[ECS Services]
    C1 --> D[PostgreSQL]
```

## Folder Structure
```
DemoSkills/
  README.md              # Project overview and status
  PLAN.md                # Development roadmap
  GOALS.md               # Short/long-term goals
  TECH_STACK.md          # Technology details
  SETUP_NOTES.md         # Environment setup
  AI_NOTES.md            # AI development guide

  apis/                  # REST API projects
    EmployeeAPI/
      Controllers/
      Models/
    CompanyAPI/
      src/
      tests/
    PayAPI/
      Controllers/
      Models/
    TaxCalculatorAPI/
      Controllers/
      Models/
    ReportAPI/
      src/
      tests/

  dashboard/             # Static UI (nginx)
    index.html
    styles.css
    app.js
    Dockerfile

  lambdas/               # Serverless functions
    TimeClockLambda/
      src/
      tests/
    VacationLambda/
      src/
      tests/

  docker/                # Containerization (legacy experiments)
    api-base/
    services/

  .harness/              # CI/CD pipelines and helpers
  infra/                 # Optional infrastructure notes/scripts
```

## Component Details

### API Services
- EmployeeAPI: Employee management
- CompanyAPI: Company information
- PayAPI: Payroll processing
- TaxCalculatorAPI: Tax computation and rules
- ReportAPI: Reporting and analytics

### Lambda Functions
- TimeClockLambda: Time tracking
- VacationLambda: Leave management

### Infrastructure
- Docker: Container definitions
- Harness: Build/deploy pipelines for ECS
- AWS: ALB + ECS (Fargate) + ECR + RDS/PostgreSQL

## API Structure

### Common Patterns
- Authentication: JWT (planned)
- Error Handling: Standardized
- Logging: Structured JSON
- Documentation: OpenAPI/Swagger

### Key Endpoints
Each API follows RESTful conventions:
- GET /api/[resource]
- POST /api/[resource]
- PUT /api/[resource]/{id}
- DELETE /api/[resource]/{id}

## Development Workflow
1. Local Development
   - Individual API projects
   - Docker Compose for services
2. Testing
   - Unit tests per project
   - Integration tests
3. Deployment
   - Harness pipelines
   - AWS infrastructure

### Test Folder Layout
Every component that has a `tests/` folder should include the following subfolders:

- `tests/unit/`
- `tests/integration/`
- `tests/bdd/features/`
- `tests/bdd/steps/`

This layout applies to each API (e.g., `apis/EmployeeAPI/tests/...`) and each lambda (e.g., `lambdas/TimeClockLambda/tests/...`).
