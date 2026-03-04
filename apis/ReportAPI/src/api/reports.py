"""Report API routes (like controller endpoints in .NET)."""

from fastapi import APIRouter
from fastapi import Depends
from shared_security_py.dependencies import get_current_principal

from ..schemas.health import HealthResponse

router = APIRouter(prefix="/api/v1/reports", tags=["reports"])

@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")


@router.get("/")
def get_reports(_principal = Depends(get_current_principal)) -> list[dict]:
    return []

