"""Report API routes (like controller endpoints in .NET)."""

from fastapi import APIRouter
from fastapi import Depends
from shared_security_py.authorization import build_require_permission
from shared_security_py.constants import PERMISSION_REPORT_READ
from shared_security_py.dependencies import get_current_principal

from ..db.deps import get_db
from ..schemas.health import HealthResponse

router = APIRouter(prefix="/api/v1/reports", tags=["reports"])

require_report_read = build_require_permission(
    PERMISSION_REPORT_READ,
    get_db_dependency=get_db,
    get_principal_dependency=get_current_principal,
)

@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")


@router.get("/")
def get_reports(_auth_context = Depends(require_report_read)) -> list[dict]:
    return []

