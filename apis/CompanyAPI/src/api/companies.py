"""Company API routes (like controller endpoints in .NET)."""
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import func
from sqlalchemy.orm import Session
from shared_security_py.dependencies import get_current_principal

from ..schemas.company import CompanyCreate, CompanyPatch, CompanyRead
from ..schemas.health import HealthResponse
from ..db.deps import get_db
# from ..models import Company as CompanyModel
from ..services.companies import CompanyService

router = APIRouter(prefix="/api/v1/companies", tags=["companies"])

@router.get("/health", response_model=HealthResponse, tags=["ops"])
def health() -> HealthResponse:
    return HealthResponse(status="ok", service="companyapi")

@router.get("/", response_model=list[CompanyRead])
def get_companies(
    db: Session = Depends(get_db),
    _principal = Depends(get_current_principal),
) -> list[CompanyRead]:
    service = CompanyService(db)
    return service.list_companies()

@router.get("/{company_id}", response_model=CompanyRead)
def get_company(company_id: UUID, db: Session = Depends(get_db)) -> CompanyRead:
    service = CompanyService(db)
    company = service.get_company_by_id(company_id)
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    return CompanyRead.model_validate(company)

@router.get("/by-short-code/{short_code}", response_model=list[CompanyRead])
def get_companies_by_short_code(short_code: str, db: Session = Depends(get_db)) -> list[CompanyRead]:
    service = CompanyService(db)
    return service.get_company_by_short_code(short_code)

@router.post("/", response_model=CompanyRead, status_code=status.HTTP_201_CREATED)
def create_company(payload: CompanyCreate, db: Session = Depends(get_db)) -> CompanyRead:
    service = CompanyService(db)
    new_company = service.create_company(payload)
    return CompanyRead.model_validate(new_company)

@router.patch("/{company_id}", response_model=CompanyRead)
def update_company(company_id: UUID, payload: CompanyPatch, db: Session = Depends(get_db)) -> CompanyRead:
    service = CompanyService(db)
    company = service.update_company(company_id, payload)
    return CompanyRead.model_validate(company)

@router.delete("/{company_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_company(company_id: UUID, db: Session = Depends(get_db)) -> None:
    service = CompanyService(db)
    service.delete_company(company_id)
    return None
