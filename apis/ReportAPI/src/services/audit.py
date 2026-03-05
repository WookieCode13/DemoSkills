"""Audit logging service."""
from datetime import datetime, timezone
from typing import Any
from uuid import UUID

from sqlalchemy.orm import Session

from ..models import AuditLog as AuditLogModel
from ..repositories.audit import AuditLogRepository


class AuditService:
    def __init__(self, db: Session) -> None:
        self._db = db
        self._audit_repo = AuditLogRepository(db)

    def add_audit_log(
        self,
        entity_type: str,
        entity_id: UUID,
        action: str,
        performed_by: str = "system",
        changed_fields: list[str] | None = None,
        changes: dict[str, Any] | None = None,
        note: str | None = None,
        correlation_id: str | None = None,
    ) -> None:
        audit_log = AuditLogModel(
            entity_id=entity_id,
            entity_type=f"ReportAPI.{entity_type}",  # Shared audit table: prefix with service name to avoid collisions.
            action=action,
            occurred_utc=datetime.now(timezone.utc),
            performed_by=performed_by,
            changed_fields=changed_fields,
            changes=changes,
            note=note,
            correlation_id=correlation_id,
        )
        self._audit_repo.add_audit_log(audit_log)
