"""Company API routes (like controller endpoints in .NET)."""
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import func
from sqlalchemy.orm import Session

from ..schemas.company import CompanyCreate, CompanyPatch, CompanyRead
from ..db.deps import get_db
from ..models import Company as CompanyModel

router = APIRouter(prefix="/companies", tags=["companies"])

@router.get("/", response_model=list[CompanyRead])
def get_companies(db: Session = Depends(get_db)) -> list[CompanyRead]:
    companies = db.query(CompanyModel).all()
    return [CompanyRead.model_validate(c) for c in companies]

@router.get("/{company_id}", response_model=CompanyRead)
def get_company(company_id: str, db: Session = Depends(get_db)) -> CompanyRead:
    company = db.query(CompanyModel).filter(CompanyModel.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    return CompanyRead.model_validate(company)

@router.post("/", response_model=CompanyRead, status_code=status.HTTP_201_CREATED)
def create_company(payload: CompanyCreate, db: Session = Depends(get_db)) -> CompanyRead:
    new_company = CompanyModel(
        short_code=payload.short_code,
        name=payload.name,
        industry=payload.industry,
        email=payload.email,
        phone=payload.phone,
    )
    db.add(new_company)
    db.commit()
    db.refresh(new_company)
    return CompanyRead.model_validate(new_company)

@router.patch("/{company_id}", response_model=CompanyRead)
def update_company(company_id: str, payload: CompanyPatch, db: Session = Depends(get_db)) -> CompanyRead:
    company = db.query(CompanyModel).filter(CompanyModel.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    company.name = payload.name
    company.industry = payload.industry
    company.email = payload.email
    company.phone = payload.phone
    db.commit()
    db.refresh(company)
    return CompanyRead.model_validate(company)

@router.delete("/{company_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_company(company_id: str, db: Session = Depends(get_db)) -> None:
    company = db.query(CompanyModel).filter(CompanyModel.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="Company not found")
    company.deleted_utc = func.now()
    db.commit()

