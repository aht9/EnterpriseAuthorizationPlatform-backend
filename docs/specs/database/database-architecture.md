# Database Architecture

Version: 1.0

Status: Approved

---

# Database Philosophy

Database is not storage.

Database is part of security architecture.

---

# Primary Database

PostgreSQL

Version: 17+

---

# Design Principles

- Multi Tenant First
- Security First
- Audit First
- Event Driven
- Immutable History

---

# Persistence Contexts

Identity Database

Authorization Database

Policy Database

Audit Database

---

# Shared Database Policy

Allowed:

Authorization
Policy

Forbidden:

Cross Service Tables

---

# Cross Service Access

Services NEVER access another service database.

Communication:

API
or
Events

Only.

---

# Aggregate Rule

Every Aggregate Root must contain:

Id
TenantId
CreatedAt
UpdatedAt
Version

---

# Concurrency

Optimistic Concurrency Required

Column:

RowVersion

Mandatory.

---

# Soft Delete

Required

Fields:

DeletedAt
DeletedBy

---

# Hard Delete

Forbidden

Except:

GDPR Workflow