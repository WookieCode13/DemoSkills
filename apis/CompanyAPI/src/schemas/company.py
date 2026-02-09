"""Pydantic models for company API requests and responses (like request/response contracts)."""
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field


class CompanyRead(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: UUID = Field(..., examples=["7b64a7c4-71de-4b2a-a3b7-9d4eaa2f6b3b"])
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])


class CompanyCreate(BaseModel):
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])
