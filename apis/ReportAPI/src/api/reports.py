"""Report API routes (like controller endpoints in .NET)."""

from fastapi import APIRouter

from ..schemas.health import HealthResponse

router = APIRouter(prefix="/api/v1/reports", tags=["reports"])

@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")

