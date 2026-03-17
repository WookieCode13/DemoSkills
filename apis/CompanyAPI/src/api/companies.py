"""Company API routes (like controller endpoints in .NET)."""
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from shared_security_py.authorization import build_require_permission
from shared_security_py.constants import (
    PERMISSION_COMPANY_CREATE,
    PERMISSION_COMPANY_DELETE,
    PERMISSION_COMPANY_READ,
    PERMISSION_COMPANY_UPDATE,
)
from shared_security_py.dependencies import get_current_principal

from ..db.deps import get_db
from ..schemas.company import CompanyCreate, CompanyPatch, CompanyRead
from ..schemas.health import HealthResponse
from ..services.companies import CompanyService

router = APIRouter(prefix="/api/v1/companies", tags=["companies"])

require_company_read = build_require_permission(
    PERMISSION_COMPANY_READ,
    get_db_dependency=get_db,
    get_principal_dependency=get_current_principal,
)
require_company_create = build_require_permission(
    PERMISSION_COMPANY_CREATE,
    get_db_dependency=get_db,
    get_principal_dependency=get_current_principal,
)
require_company_update = build_require_permission(
    PERMISSION_COMPANY_UPDATE,
    get_db_dependency=get_db,
    get_principal_dependency=get_current_principal,
)
require_company_delete = build_require_permission(
    PERMISSION_COMPANY_DELETE,
    get_db_dependency=get_db,
    get_principal_dependency=get_current_principal,
)

@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="companyapi")

@router.get("/", response_model=list[CompanyRead])
def get_companies(
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_read),
) -> list[CompanyRead]:
    service = CompanyService(db)
    return service.list_companies()

@router.get("/{company_id}", response_model=CompanyRead)
def get_company(
    company_id: UUID,
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_read),
) -> CompanyRead:
    service = CompanyService(db)
    company = service.get_company_by_id(company_id)
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    return CompanyRead.model_validate(company)

@router.get("/by-short-code/{short_code}", response_model=CompanyRead)
def get_company_by_short_code(
    short_code: str,
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_read),
) -> CompanyRead:
    service = CompanyService(db)
    company = service.get_company_by_short_code(short_code)
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    return CompanyRead.model_validate(company)

@router.post("/", response_model=CompanyRead, status_code=status.HTTP_201_CREATED)
def create_company(
    payload: CompanyCreate,
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_create),
) -> CompanyRead:
    service = CompanyService(db)
    new_company = service.create_company(payload)
    return CompanyRead.model_validate(new_company)

@router.patch("/{company_id}", response_model=CompanyRead)
def update_company(
    company_id: UUID,
    payload: CompanyPatch,
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_update),
) -> CompanyRead:
    service = CompanyService(db)
    company = service.update_company(company_id, payload)
    return CompanyRead.model_validate(company)

@router.delete("/{company_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_company(
    company_id: UUID,
    db: Session = Depends(get_db),
    _auth_context = Depends(require_company_delete),
) -> None:
    service = CompanyService(db)
    service.delete_company(company_id)
    return None
