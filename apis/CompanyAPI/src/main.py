from typing import Annotated

import os
from fastapi import APIRouter, Body, FastAPI, HTTPException, Path, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import RedirectResponse
from pydantic import BaseModel, Field


_build_branch = os.getenv("BUILD_BRANCH", "local")
_root_path = os.getenv("ROOT_PATH", "")

app = FastAPI(
    title="CompanyAPI",
    version=f"api v1 ({_build_branch})",
    description=f"DemoSkills Company API (FastAPI). Build branch: {_build_branch}.",
    docs_url="/docs",
    openapi_url="/openapi.json",
    root_path=_root_path,
)

_cors_origins = ["http://longranch.com", "http://dashboard.longranch.com"]
app.add_middleware(
    CORSMiddleware,
    allow_origins=_cors_origins,
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


class HealthResponse(BaseModel):
    status: str
    service: str


class TodoResponse(BaseModel):
    status: str
    message: str


class Company(BaseModel):
    id: int = Field(..., examples=[1])
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])


class CompanyCreateRequest(BaseModel):
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])


_companies: dict[int, Company] = {}
_next_company_id = 1


@app.get("/", tags=["ops"])
def root():
    return {
        "service": "companyapi",
        "docs": "/companies/docs",
        "openapi": "/companies/openapi.json",
        "health": "/health",
    }


@app.get("/health", response_model=HealthResponse, tags=["ops"])
def root_health() -> HealthResponse:
    return HealthResponse(status="ok", service="companyapi")


router = APIRouter(prefix="/api/v1/companies", tags=["companies"])


@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="companyapi")


@router.get("", response_model=TodoResponse, summary="List companies")
def list_companies() -> TodoResponse:
    return TodoResponse(status="todo", message="TODO: load companies from the database")


@router.get("/{company_id}", response_model=TodoResponse, summary="Get company by id")
def get_company(
    company_id: Annotated[int, Path(..., ge=1, description="Company ID")]
) -> TodoResponse:
    return TodoResponse(
        status="todo",
        message=f"TODO: load company {company_id} from the database",
    )


@router.post(
    "",
    response_model=Company,
    status_code=status.HTTP_201_CREATED,
    summary="Create company",
)
def create_company(
    request: Annotated[CompanyCreateRequest, Body(..., description="Company to create")]
) -> Company:
    global _next_company_id

    new_company = Company(
        id=_next_company_id,
        name=request.name.strip(),
        industry=(request.industry.strip() if request.industry else None),
    )
    _companies[new_company.id] = new_company
    _next_company_id += 1
    return new_company


app.include_router(router)


if __name__ == "__main__":
    import os

    import uvicorn

    port = int(os.getenv("PORT", "8081"))
    uvicorn.run(app, host="0.0.0.0", port=port)
