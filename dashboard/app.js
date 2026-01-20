const protocol = window.location.protocol || "http:";
const host = window.location.hostname || "longranch.local";
const params = new URLSearchParams(window.location.search);
const overrideBaseDomain = params.get("baseDomain");
const baseDomain =
  overrideBaseDomain || (host.startsWith("dashboard.") ? host.replace("dashboard.", "") : host);

const services = [
  {
    name: "Employee",
    healthUrl: `${protocol}//employee.${baseDomain}/api/v1/employees/health`,
    swaggerUrl: `${protocol}//employee.${baseDomain}/swagger`,
  },
  {
    name: "Company",
    healthUrl: `${protocol}//company.${baseDomain}/api/v1/companies/health`,
    swaggerUrl: `${protocol}//company.${baseDomain}/docs`,
  },
  {
    name: "Pay",
    healthUrl: `${protocol}//pay.${baseDomain}/api/v1/pay/health`,
    swaggerUrl: `${protocol}//pay.${baseDomain}/swagger`,
  },
  {
    name: "Tax",
    healthUrl: `${protocol}//tax.${baseDomain}/api/v1/taxes/health`,
    swaggerUrl: `${protocol}//tax.${baseDomain}/swagger`,
  },
  {
    name: "Report",
    healthUrl: `${protocol}//report.${baseDomain}/api/v1/reports/health`,
    swaggerUrl: `${protocol}//report.${baseDomain}/docs`,
  },
];

const cardsEl = document.getElementById("cards");
const refreshBtn = document.getElementById("refresh-btn");

async function fetchWithTimeout(url, timeoutMs = 5000) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const res = await fetch(url, { signal: controller.signal });
    clearTimeout(timer);
    return res;
  } catch (err) {
    clearTimeout(timer);
    throw err;
  }
}

async function checkService(service) {
  try {
    const res = await fetchWithTimeout(service.healthUrl);
    const ok = res.ok;
    const text = await res.text();
    return {
      status: ok ? "ok" : "fail",
      statusCode: res.status,
      detail: text.slice(0, 120) || res.statusText,
    };
  } catch (err) {
    return {
      status: "fail",
      statusCode: "ERR",
      detail: err.name === "AbortError" ? "Timed out" : err.message,
    };
  }
}

function render(servicesState) {
  cardsEl.innerHTML = servicesState
    .map(({ service, result }) => {
      const statusClass =
        result.status === "ok" ? "status ok" : result.status === "warn" ? "status warn" : "status fail";
      return `
        <article class="card">
          <div style="display:flex;justify-content:space-between;align-items:center;">
            <h3>${service.name}</h3>
            <span class="${statusClass}">${result.status.toUpperCase()}</span>
          </div>
          <p class="meta">Code: ${result.statusCode} · ${result.detail}</p>
          <div class="links">
            <a href="${service.healthUrl}" target="_blank" rel="noreferrer">Health</a>
            <a href="${service.swaggerUrl}" target="_blank" rel="noreferrer">Docs</a>
          </div>
        </article>
      `;
    })
    .join("");
}

async function loadStatuses() {
  refreshBtn.disabled = true;
  refreshBtn.textContent = "Refreshing...";
  const results = await Promise.all(
    services.map(async (svc) => ({ service: svc, result: await checkService(svc) }))
  );
  render(results);
  refreshBtn.disabled = false;
  refreshBtn.textContent = "Refresh";
}

refreshBtn.addEventListener("click", loadStatuses);

loadStatuses();
