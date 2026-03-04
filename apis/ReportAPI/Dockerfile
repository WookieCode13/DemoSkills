# ReportAPI (Python/FastAPI)

FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PYTHONPATH=/app:/app/shared

WORKDIR /app

COPY apis/ReportAPI/requirements.txt ./requirements.txt
RUN pip install --no-cache-dir -r ./requirements.txt

COPY apis/ReportAPI/src ./src
COPY Shared/Security/py/shared_security_py ./shared/shared_security_py

EXPOSE 8084
ENV PORT=8084

CMD ["uvicorn", "src.main:app", "--host", "0.0.0.0", "--port", "8084"]
