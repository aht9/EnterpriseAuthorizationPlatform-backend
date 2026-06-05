# Observability Strategy

Version: 1.0

---

# Pillars

Logs

Metrics

Traces

Events

---
# Logging

Structured Logging

JSON

Required

---
# Correlation

همه درخواست‌ها:

CorrelationId

RequestId

TenantId
دارند.

---

# OpenTelemetry

Mandatory

---

# Trace Flow

Kong

↓

Authorization

↓

OPA

↓

Redis

↓

PostgreSQL

---

همه باید در یک Trace باشند.

---

# Monitoring Stack

OpenTelemetry

Prometheus

Grafana

Loki

Tempo

---

# Golden Signals

Latency

Traffic

Errors

Saturation