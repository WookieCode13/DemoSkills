from dataclasses import dataclass, field
from typing import Any


@dataclass
class Principal:
    sub: str
    email: str | None = None
    groups: list[str] = field(default_factory=list)
    claims: dict[str, Any] = field(default_factory=dict)
