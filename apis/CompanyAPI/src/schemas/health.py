"""Pydantic models for health endpoints (like response DTOs)."""
from pydantic import BaseModel


class HealthResponse(BaseModel):
    status: str
    service: str
