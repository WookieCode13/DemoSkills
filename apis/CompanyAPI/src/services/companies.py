"""Company business logic (service layer)."""
import logging
from uuid import UUID

from fastapi import HTTPException
from sqlalchemy import func
from sqlalchemy.orm import Session

from ..models import Company as CompanyModel
from ..repositories.companies import CompanyRepository
from ..schemas.company import CompanyCreate, CompanyPatch, CompanyRead
from .audit import AuditService

logger = logging.getLogger(__name__)

class CompanyService:
    def __init__(self, db: Session) -> None:
        self._db = db
        self._company_repo = CompanyRepository(db)
        self._audit_service = AuditService(db)

    def list_companies(self) -> list[CompanyRead]:
        logger.info("Listing companies")
        companies = self._company_repo.list_companies()
        logger.info("Listed companies count=%s", len(companies))
        return [CompanyRead.model_validate(company) for company in companies]
    
    def get_company_by_id(self, company_id: UUID) -> CompanyRead | None:
        logger.info("Getting company by id company_id=%s", company_id)
        company = self._company_repo.get_company_by_id(company_id)
        if not company:
            logger.warning("Company not found by id company_id=%s", company_id)
            return None
        logger.info("Found company by id company_id=%s short_code=%s", company_id, company.short_code)
        return CompanyRead.model_validate(company) if company else None
    
    def get_company_by_short_code(self, short_code: str) -> CompanyRead | None:
        logger.info("Getting company by short_code short_code=%s", short_code)
        company = self._company_repo.get_company_by_short_code(short_code)
        if not company:
            logger.warning("Company not found by short_code short_code=%s", short_code)
            return None
        logger.info("Found company by short_code short_code=%s company_id=%s", short_code, company.id)
        return CompanyRead.model_validate(company) if company else None
    
    def create_company(self, payload: CompanyCreate) -> CompanyRead:
        logger.info("Creating company short_code=%s name=%s", payload.short_code, payload.name)
        new_company = CompanyModel(
            short_code=payload.short_code,
            name=payload.name,
            industry=payload.industry,
            email=payload.email,
            phone=payload.phone,
        )
        try:
            self._company_repo.add_company(new_company)
            # flush to get the new company ID for the audit log, 
            # but commit+refresh is deferred until after the audit log is added to keep it all in one transaction.
            self._db.flush()
            self._audit_service.add_audit_log(
                entity_type="company",
                entity_id=new_company.id,
                action="created",
                changed_fields=None,
            )
            self._db.commit()
            self._db.refresh(new_company)
        except Exception:
            logger.exception("Company or Audit log failed for create company_id=%s", new_company.id)
            self._db.rollback()
            raise HTTPException(status_code=500, detail="Create company failed")
        logger.info("Created company company_id=%s short_code=%s", new_company.id, new_company.short_code)
        return CompanyRead.model_validate(new_company)
    
    def update_company(self, company_id: UUID, payload: CompanyPatch) -> CompanyRead:
        logger.info("Updating company company_id=%s", company_id)
        company = self._company_repo.get_company_by_id(company_id)
        if not company:
            logger.warning("Company not found for update company_id=%s", company_id)
            raise HTTPException(status_code=404, detail="Company not found")
        data = payload.model_dump(exclude_unset=True)
        if "name" in data:
            if data["name"] is None:
                logger.warning("Invalid null name in update payload company_id=%s", company_id)
                raise HTTPException(status_code=400, detail="name cannot be null")
            company.name = data["name"]
        if "industry" in data:
            company.industry = data["industry"]
        if "email" in data:
            company.email = data["email"]
        if "phone" in data:
            company.phone = data["phone"]
        if data:
            company.updated_utc = func.now()
        try:
            self._audit_service.add_audit_log(
                entity_type="company",
                entity_id=company_id,
                action="updated",
                changed_fields=",".join(data.keys()) if data else None,
            )
            self._company_repo.update_company(company)
            self._db.commit()
            self._db.refresh(company)
        except Exception:
            logger.exception("Company or Audit log failed for update company_id=%s", company_id)
            self._db.rollback()
            raise HTTPException(status_code=500, detail="Update company failed")
        logger.info("Updated company company_id=%s changed_fields=%s", company_id, ",".join(data.keys()) if data else "")
        return CompanyRead.model_validate(company)
    
    def delete_company(self, company_id: UUID) -> None:
        logger.info("Deleting company company_id=%s", company_id)
        company = self._company_repo.get_company_by_id(company_id)
        if not company:
            logger.warning("Company not found for delete company_id=%s", company_id)
            raise HTTPException(status_code=404, detail="Company not found")
        company.deleted_utc = func.now()
        try:
            self._audit_service.add_audit_log(
                entity_type="company",
                entity_id=company_id,
                action="deleted",
                changed_fields=None,
            )
            self._company_repo.update_company(company)
            self._db.commit()
            self._db.refresh(company)
        except Exception:
            logger.exception("Company or Audit log failed for delete company_id=%s", company_id)
            self._db.rollback()
            raise HTTPException(status_code=500, detail="Delete company failed")
        logger.info("Deleted company company_id=%s", company_id)
        return None
        
