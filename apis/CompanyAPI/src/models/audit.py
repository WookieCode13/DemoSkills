"""Audit Log Model."""
from sqlalchemy import Column, DateTime, String, Text
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.sql import func

from ..db.base import Base


class AuditLog(Base):
    __tablename__ = "audit_log"

    id = Column(UUID(as_uuid=True), primary_key=True, server_default=func.uuid_generate_v4())
    entity_type = Column(String(50), nullable=False)
    entity_id = Column(UUID(as_uuid=True), nullable=False)
    action = Column(String(50), nullable=False)
    occurred_utc = Column(DateTime(timezone=True), nullable=False, server_default=func.now())
    performed_by = Column(String(255), nullable=False, server_default="system")
    changed_fields = Column(JSONB, nullable=True)
    note = Column(Text, nullable=True)
    correlation_id = Column(Text, nullable=True)


