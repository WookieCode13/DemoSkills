FROM node:20-alpine AS ui-builder

ARG DASHBOARD_UI_VARIANT=react-ts-v1

WORKDIR /workspace
COPY . .

RUN if [ -f "uis/${DASHBOARD_UI_VARIANT}/package.json" ]; then \
      cd "uis/${DASHBOARD_UI_VARIANT}" && npm install --no-fund --no-audit && npm run build; \
    fi

FROM nginx:1.27-alpine

ARG DASHBOARD_UI_VARIANT=react-ts-v1

COPY . /opt/dashboard-src
COPY --from=ui-builder /workspace /opt/dashboard-build

RUN if [ -d "/opt/dashboard-build/uis/${DASHBOARD_UI_VARIANT}/dist" ]; then \
      cp -r /opt/dashboard-build/uis/${DASHBOARD_UI_VARIANT}/dist/. /usr/share/nginx/html/; \
    else \
      cp /opt/dashboard-src/uis/${DASHBOARD_UI_VARIANT}/index.html /usr/share/nginx/html/index.html \
      && cp /opt/dashboard-src/uis/${DASHBOARD_UI_VARIANT}/styles.css /usr/share/nginx/html/styles.css \
      && cp /opt/dashboard-src/uis/${DASHBOARD_UI_VARIANT}/app.js /usr/share/nginx/html/app.js; \
    fi
