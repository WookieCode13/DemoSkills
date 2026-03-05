"""FastAPI dependency for a per-request DB session (like scoped DbContext)."""
from typing import Generator
from sqlalchemy.orm import Session

from .database import SessionLocal

def get_db() -> Generator[Session, None, None]:
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()  # ensure the database session is closed after the request is done, returns connection to pool.
