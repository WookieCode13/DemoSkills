import React from "react";

type ServiceDefinition = {
  name: string;
  category: string;
  healthUrl: string;
  docsUrl: string;
};

type ServiceResult = {
  status: "ok" | "fail";
  statusCode: number | "ERR";
  detail: string;
};

const protocol = window.location.protocol || "http:";
const host = window.location.hostname || "longranch.wookie";
const params = new URLSearchParams(window.location.search);
const overrideBaseDomain = params.get("baseDomain");
const baseDomain =
  overrideBaseDomain || (host.startsWith("dashboard.") ? host.replace("dashboard.", "") : host);

const services: ServiceDefinition[] = [
  {
    name: "Admin",
    category: "Employee + Company",
    healthUrl: `${protocol}//employee.${baseDomain}/api/v1/employees/health`,
    docsUrl: `${protocol}//employee.${baseDomain}/swagger`,
  },
  {
    name: "Pay",
    category: "Payroll + Tax",
    healthUrl: `${protocol}//pay.${baseDomain}/api/v1/pay/health`,
    docsUrl: `${protocol}//pay.${baseDomain}/swagger`,
  },
  {
    name: "Report",
    category: "Reporting",
    healthUrl: `${protocol}//report.${baseDomain}/api/v1/reports/health`,
    docsUrl: `${protocol}//report.${baseDomain}/swagger`,
  },
];

async function fetchWithTimeout(url: string, timeoutMs = 5000) {
  const controller = new AbortController();
  const timer = window.setTimeout(() => controller.abort(), timeoutMs);
  try {
    const res = await fetch(url, { signal: controller.signal });
    window.clearTimeout(timer);
    return res;
  } catch (error) {
    window.clearTimeout(timer);
    throw error;
  }
}

async function checkService(service: ServiceDefinition): Promise<ServiceResult> {
  try {
    const response = await fetchWithTimeout(service.healthUrl);
    const body = await response.text();
    return {
      status: response.ok ? "ok" : "fail",
      statusCode: response.status,
      detail: body.slice(0, 140) || response.statusText,
    };
  } catch (error) {
    const err = error as Error & { name?: string };
    return {
      status: "fail",
      statusCode: "ERR",
      detail: err.name === "AbortError" ? "Timed out" : err.message,
    };
  }
}

function formatStatusLabel(result: ServiceResult) {
  return result.status === "ok" ? "Healthy" : "Attention";
}

export default function App() {
  const [results, setResults] = React.useState<Record<string, ServiceResult>>({});
  const [isRefreshing, setIsRefreshing] = React.useState(true);
  const [lastChecked, setLastChecked] = React.useState<string>("Starting...");

  const refresh = React.useCallback(async () => {
    setIsRefreshing(true);
    const entries = await Promise.all(
      services.map(async (service) => [service.name, await checkService(service)] as const)
    );
    setResults(Object.fromEntries(entries));
    setLastChecked(new Date().toLocaleTimeString());
    setIsRefreshing(false);
  }, []);

  React.useEffect(() => {
    void refresh();
  }, [refresh]);

  const healthyCount = services.filter((service) => results[service.name]?.status === "ok").length;

  return (
    <div className="shell">
      <header className="hero">
        <div className="hero-copy">
          <p className="eyebrow">DemoSkills Control Room</p>
          <h1>React dashboard variant for API visibility.</h1>
          <p className="lede">
            Monitor the consolidated service surface, verify health responses, and jump directly into
            docs from one place.
          </p>
        </div>
        <div className="hero-panel">
          <div className="metric">
            <span className="metric-label">Healthy</span>
            <strong>{healthyCount}</strong>
            <span className="metric-total">/ {services.length}</span>
          </div>
          <button type="button" onClick={() => void refresh()} disabled={isRefreshing}>
            {isRefreshing ? "Refreshing..." : "Refresh"}
          </button>
          <p className="timestamp">Last checked: {lastChecked}</p>
        </div>
      </header>

      <main className="grid">
        {services.map((service, index) => {
          const result = results[service.name];
          const statusClass = result?.status === "ok" ? "ok" : "fail";

          return (
            <article className="card" key={service.name} style={{ animationDelay: `${index * 120}ms` }}>
              <div className="card-top">
                <div>
                  <p className="card-category">{service.category}</p>
                  <h2>{service.name}</h2>
                </div>
                <span className={`pill ${statusClass}`}>
                  {result ? formatStatusLabel(result) : "Pending"}
                </span>
              </div>

              <p className="code-line">
                {result ? (
                  <>
                    Code <strong>{result.statusCode}</strong> · {result.detail}
                  </>
                ) : (
                  "Waiting for health response..."
                )}
              </p>

              <div className="card-actions">
                <a href={service.healthUrl} target="_blank" rel="noreferrer">
                  Health
                </a>
                <a href={service.docsUrl} target="_blank" rel="noreferrer">
                  Swagger
                </a>
              </div>
            </article>
          );
        })}
      </main>
    </div>
  );
}
