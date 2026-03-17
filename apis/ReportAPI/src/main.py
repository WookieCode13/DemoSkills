import os
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

if __package__ in (None, ""):
    from api.reports import router as reports_router
    from core.logger import configure_logging
    from schemas.health import HealthResponse
else:
    from .api.reports import router as reports_router
    from .core.logger import configure_logging
    from .schemas.health import HealthResponse

_build_branch = os.getenv("BUILD_BRANCH", "local")
_root_path = os.getenv("ROOT_PATH", "")
configure_logging()


app = FastAPI(
    title="ReportAPI",
    version=f"0.1.0 ({_build_branch})",
    description=f"DemoSkills Report API (FastAPI). Build branch: {_build_branch}.",
    docs_url="/docs",
    openapi_url="/openapi.json",
    root_path=_root_path,
)

_env = os.getenv("ASPNETCORE_ENVIRONMENT", "production")
_cors_origins = [
    "http://longranch.com",
    "http://dashboard.longranch.com",
    "https://longranch.com",
    "https://dashboard.longranch.com",
]
if _env.lower() == "development":
    _cors_origins.extend(["http://longranch.wookie", "http://dashboard.longranch.wookie"])
app.add_middleware(
    CORSMiddleware,
    allow_origins=_cors_origins,
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/", tags=["ops"])
def root():
    return {
        "service": "reportapi",
        "docs": "/reports/docs",
        "openapi": "/reports/openapi.json",
        "health": "/health",
    }


@app.get("/health", response_model=HealthResponse, tags=["ops"])
def root_health() -> HealthResponse:
    return HealthResponse(status="ok", service="reportapi")


app.include_router(reports_router)

if __name__ == "__main__":
    import uvicorn

    port = int(os.getenv("PORT", "8084"))
    uvicorn.run(app, host="0.0.0.0", port=port)
