"""Pydantic models for reports API requests and responses (like request/response contracts)."""
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field

# todo temp code from cpmpany, will need to be updated. 

class ReportsRead(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: UUID = Field(..., examples=["7b64a7c4-71de-4b2a-a3b7-9d4eaa2f6b3b"])
    short_code: str = Field(..., min_length=10, max_length=10, examples=["DEMOSKILLS"])
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])
    email: str | None = Field(default=None, examples=["info@demoskills.com"])
    phone: str | None = Field(default=None, examples=["555-1234"])


class ReportsCreate(BaseModel):
    short_code: str = Field(..., min_length=10, max_length=10, examples=["DEMOSKILLS"])
    name: str = Field(..., min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])
    email: str | None = Field(default=None, examples=["info@demoskills.com"])
    phone: str | None = Field(default=None, examples=["555-1234"])


class ReportsPatch(BaseModel):
    name: str | None = Field(default=None, min_length=1, examples=["Demo Skills LLC"])
    industry: str | None = Field(default=None, examples=["Demo Skills 123"])
    email: str | None = Field(default=None, examples=["info@demoskills.com"])
    phone: str | None = Field(default=None, examples=["555-1234"])

