from typing import Annotated

import os
from fastapi import APIRouter, Body, FastAPI, HTTPException, Path, status
from pydantic import BaseModel, Field


_build_branch = os.getenv("BUILD_BRANCH", "local")

app = FastAPI(
    title="CompanyAPI",
    version=f"api v1 ({_build_branch})",
    description=f"DemoSkills Company API (FastAPI). Build branch: {_build_branch}.",
    docs_url="/companies/docs",
    openapi_url="/companies/openapi.json",
)


class HealthResponse(BaseModel):
    status: str
    service: str


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


@router.get(
    "",
    response_model=list[Company],
    summary="List companies",
)
def list_companies() -> list[Company]:
    # TODO: replace in-memory stub with real data source.
    return sorted(_companies.values(), key=lambda c: c.id)


@router.get(
    "/{company_id}",
    response_model=Company,
    summary="Get company by id",
)
def get_company(
    company_id: Annotated[int, Path(..., ge=1, description="Company ID")]
) -> Company:
    company = _companies.get(company_id)
    if company is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Company {company_id} not found",
        )
    return company


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
