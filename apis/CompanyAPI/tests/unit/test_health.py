from fastapi.testclient import TestClient

from src.main import app


def test_health_returns_ok():
    client = TestClient(app)
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "ok"


def test_companies_list_returns_seed_data():
    client = TestClient(app)
    response = client.get("/api/v1/companies")
    assert response.status_code == 200
    data = response.json()
    assert isinstance(data, list)
    assert len(data) >= 2
    assert data[0]["id"] == 1


def test_create_company_returns_201_and_assigns_id():
    client = TestClient(app)
    response = client.post(
        "/api/v1/companies",
        json={"name": "ACME", "industry": "Manufacturing"},
    )
    assert response.status_code == 201
    data = response.json()
    assert data["id"] >= 3
    assert data["name"] == "ACME"


def test_get_company_not_found_returns_404():
    client = TestClient(app)
    response = client.get("/api/v1/companies/999999")
    assert response.status_code == 404
