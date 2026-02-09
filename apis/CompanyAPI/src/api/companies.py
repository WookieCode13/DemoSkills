"""Company API routes (like controller endpoints in .NET)."""
from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from sqlalchemy import text


from ..schemas.company import Company
from ..db.deps import get_db
from ..models import Company as CompanyModel

router = APIRouter(prefix="/companies", tags=["companies"])

@router.get("/", response_model=list[Company])
def get_companies(db: Session = Depends(get_db)) -> list[Company]:
    companies = db.query(CompanyModel).all()
    return [Company.from_orm(c) for c in companies]

@router.get("/_dbtest")
def db_test(db: Session = Depends(get_db)):
    db.execute(text("SELECT 1"))
    return {"db": "ok"}

