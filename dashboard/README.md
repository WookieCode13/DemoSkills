# Dashboard UIs

Each dashboard variant lives in its own folder under `dashboard/uis/`.

Current layout:

- `uis/status-monitor-static/` - current lightweight status dashboard
- `uis/react-ts-v1/` - React + TypeScript dashboard variant

Docker build selection:

- `DASHBOARD_UI_VARIANT=react-ts-v1` builds the current default UI
- `DASHBOARD_UI_VARIANT=status-monitor-static` builds the original static UI

When adding a new UI, create a sibling folder with its own `index.html`, `styles.css`, and `app.js`
or a self-contained app entrypoint that the Dockerfile can promote into nginx content.
