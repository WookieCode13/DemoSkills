"""SQLAlchemy model for the company table (like EF entity)."""
from sqlalchemy import Column, DateTime, String, Text
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func

from ..db.base import Base


class Company(Base):
    __tablename__ = "company"

    id = Column(UUID(as_uuid=True), primary_key=True, server_default=func.uuid_generate_v4())
    short_code = Column(String(10), unique=True, nullable=False)
    name = Column(String(255), nullable=False)
    industry = Column(String(255))
    email = Column(String(255))
    phone = Column(String(20))
    created_utc = Column(DateTime(timezone=True), nullable=False, server_default=func.now())
    updated_utc = Column(DateTime(timezone=True), nullable=False, server_default=func.now())
    deleted_utc = Column(DateTime(timezone=True))
