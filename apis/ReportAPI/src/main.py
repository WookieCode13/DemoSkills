from typing import Annotated

import os
from fastapi import APIRouter, Body, FastAPI, HTTPException, Path, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import RedirectResponse
from pydantic import BaseModel, Field

_build_branch = os.getenv("BUILD_BRANCH", "local")

app = FastAPI(
    title="ReportAPI",
    version=f"0.1.0 ({_build_branch})",
    description=f"DemoSkills Report API (FastAPI). Build branch: {_build_branch}.",
    docs_url="/docs",
    openapi_url="/openapi.json",
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


class Report(BaseModel):
    id: int = Field(..., examples=[1])
    name: str = Field(..., min_length=1, examples=["Quarterly Summary"])
    description: str | None = Field(default=None, examples=["Example report"])


class ReportCreateRequest(BaseModel):
    name: str = Field(..., min_length=1, examples=["Quarterly Summary"])
    description: str | None = Field(default=None, examples=["Example report"])


_reports: dict[int, Report] = {}
_next_report_id = 1


@app.get("/", tags=["ops"])
def root():
    return {
        "service": "reportapi",
        "docs": "/reports/docs",
        "openapi": "/reports/openapi.json",
        "health": "/health",
    }


@app.get("/health", response_model=HealthResponse, tags=["ops"])
def root_health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")


router = APIRouter(prefix="/api/v1/reports", tags=["reports"])


@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")


@router.get("", response_model=list[Report], summary="List reports")
def list_reports() -> list[Report]:
    # TODO: replace in-memory stub with real data source.
    return sorted(_reports.values(), key=lambda r: r.id)


@router.get("/{report_id}", response_model=Report, summary="Get report by id")
def get_report(
    report_id: Annotated[int, Path(..., ge=1, description="Report ID")]
) -> Report:
    report = _reports.get(report_id)
    if report is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Report {report_id} not found",
        )
    return report


@router.post("", response_model=Report, status_code=status.HTTP_201_CREATED, summary="Create report")
def create_report(
    request: Annotated[ReportCreateRequest, Body(..., description="Report to create")]
) -> Report:
    global _next_report_id

    new_report = Report(
        id=_next_report_id,
        name=request.name.strip(),
        description=(request.description.strip() if request.description else None),
    )
    _reports[new_report.id] = new_report
    _next_report_id += 1
    return new_report


app.include_router(router)


if __name__ == "__main__":
    import uvicorn

    port = int(os.getenv("PORT", "8084"))
    uvicorn.run(app, host="0.0.0.0", port=port)
