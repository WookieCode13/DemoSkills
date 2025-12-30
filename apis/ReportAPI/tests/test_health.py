from fastapi.testclient import TestClient

from src.main import app


client = TestClient(app)


def test_health_api_scope():
    response = client.get("/api/v1/reports/health")
    assert response.status_code == 200
    data = response.json()
    assert data.get("status") == "ok"
    assert data.get("service") == "reportapi"
