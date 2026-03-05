"""Audit Log Repository."""
from uuid import UUID

from sqlalchemy.orm import Session

from ..models import AuditLog as AuditLogModel


class AuditLogRepository:
    def __init__(self, db: Session) -> None:
        self._db = db

    def add_audit_log(self, audit_log: AuditLogModel) -> None:
        # wrapper for default audit log add.
        # note for me: we want 1 transaction, so the commit+refresh is moved to service for entity and audit log together.
        self._db.add(audit_log)

# TODO add audit service with GET list and contracts for a future UI usage.
