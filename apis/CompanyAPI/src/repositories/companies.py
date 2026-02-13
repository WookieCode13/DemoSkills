"""Company data access helpers (repository layer)."""
from uuid import UUID

from sqlalchemy.orm import Session

from ..models import Company as CompanyModel

class CompanyRepository:
    def __init__(self, db: Session) -> None:
        self._db = db


    def list_companies(self) -> list[CompanyModel]:
        return self._db.query(CompanyModel).all()


    def get_company_by_id(self, company_id: UUID) -> CompanyModel | None:
        return self._db.query(CompanyModel).filter(CompanyModel.id == company_id).first()


    def get_company_by_short_code(self, short_code: str) -> CompanyModel | None:
        return self._db.query(CompanyModel).filter(CompanyModel.short_code == short_code).first()


    def add_company(self, company: CompanyModel) -> CompanyModel:
        # wrapper for default company add.
        self._db.add(company)
        return company


    def update_company(self, company: CompanyModel) -> CompanyModel:
        # wrapper for default company update.
        return company

# delete is usually just a soft delete by setting deleted_utc, but here's how a hard delete would look:
# def delete_company(self, company: CompanyModel) -> None:
#     self._db.delete(company)
#     self._db.commit()

