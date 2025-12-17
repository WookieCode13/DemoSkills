# CompanyAPI

Python API service for DemoSkills.

## Run locally

```bash
cd apis/CompanyAPI
python -m venv .venv
. .venv/bin/activate
pip install -r requirements.txt
uvicorn src.main:app --host 0.0.0.0 --port 8081 --reload
```

Open:

- `http://localhost:8081/health`
- `http://localhost:8081/docs` (Swagger UI)
- `http://localhost:8081/openapi.json`

