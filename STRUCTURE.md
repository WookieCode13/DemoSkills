# Project Structure

## System Overview
```mermaid
graph TD
    A[Client] --> B[API Gateway]
    B --> C1[REST APIs]
    B --> C2[Lambda Functions]
    C1 --> D[Database]
    C2 --> D
```

## Folder Structure
DemoSkills/
│
├── README.md           # Project overview and status
├── PROJECT_PLAN.md     # Development roadmap
├── TECH_STACK.md      # Technology details
├── SETUP_NOTES.md     # Environment setup
├── AI_NOTES.md        # AI development guide
│
├── /apis/             # REST API Projects
│   ├── /TaxCalculatorAPI/
│   │   ├── /src/
│   │   └── /tests/
│   ├── /EmployeeAPI/
│   │   ├── /src/
│   │   └── /tests/
│   ├── /CompanyAPI/
│   │   ├── /src/
│   │   └── /tests/
│   └── /PayAPI/
│       ├── /src/
│       └── /tests/
│
├── /lambdas/          # Serverless Functions
│   ├── /TimeClockLambda/
│   │   ├── /src/
│   │   └── /tests/
│   └── /VacationLambda/
│       ├── /src/
│       └── /tests/
│
├── /docker/           # Containerization
│   ├── /api-base/     # Base images
│   └── /services/     # Service-specific
│
├── /harness/          # (Optional) CI/CD Configuration
│   ├── /pipelines/
│   ├── /connectors/
│   └── /templates/
│
└── /infra/            # (Optional) infrastructure notes/scripts

## Component Details

### API Services
- TaxCalculatorAPI: Tax computation and rules
- EmployeeAPI: Employee management
- CompanyAPI: Company information
- PayAPI: Payroll processing

### Lambda Functions
- TimeClockLambda: Time tracking
- VacationLambda: Leave management

### Infrastructure
- Docker: Container definitions
- Harness: Deployment pipelines
- AWS: Cloud resources

## API Structure

### Common Patterns
- Authentication: JWT
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
