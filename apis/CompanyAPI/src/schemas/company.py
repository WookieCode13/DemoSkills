"""Pydantic models for company API requests and responses (like request/response contracts)."""
from pydantic import BaseModel, Field


class Company(BaseModel):
    id: int = Field(..., examples=[1])
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])


class CompanyCreateRequest(BaseModel):
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])
