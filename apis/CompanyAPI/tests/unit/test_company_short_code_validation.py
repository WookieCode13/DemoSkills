import pytest
from fastapi import HTTPException

from src.services.companies import CompanyService


class DummySession:
    pass


def build_service() -> CompanyService:
    return CompanyService(DummySession())


def test_validate_short_code_allows_uppercase_letters_and_digits() -> None:
    service = build_service()

    service._validate_short_code("DEMO123456")


@pytest.mark.parametrize(
    ("short_code", "expected_detail"),
    [
        ("DEMO_23456", "Invalid short code"),
        ("DEMO-23456", "Invalid short code"),
        ("demo123456", "Invalid short code"),
        ("AUTHCO1234", "Reserved name"),
        ("XXAUTH1234", "Reserved name"),
    ],
)
def test_validate_short_code_rejects_invalid_values(short_code: str, expected_detail: str) -> None:
    service = build_service()

    with pytest.raises(HTTPException) as exc_info:
        service._validate_short_code(short_code)

    assert exc_info.value.status_code == 400
    assert exc_info.value.detail == expected_detail
