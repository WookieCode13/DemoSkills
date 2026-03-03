"""SQLAlchemy declarative base shared by all ORM models (like EF Core model base)."""
from sqlalchemy.orm import declarative_base

Base = declarative_base()