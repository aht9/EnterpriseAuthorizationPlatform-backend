# Tenant Isolation Strategy

Version: 1.0

---

# Isolation Model

Shared Database

Shared Schema

TenantId

---

# Required

All Aggregates

must contain

TenantId

---

# Query Rule

Every Query

must contain

Tenant Filter

---

# Example

Allowed

WHERE TenantId=@TenantId

Forbidden

SELECT * FROM Invoices

without TenantId

---

# PostgreSQL RLS

Mandatory

for critical tables.

---

# RLS Example

CREATE POLICY tenant_policy
ON invoices
USING (tenant_id = current_setting('app.tenant_id'))