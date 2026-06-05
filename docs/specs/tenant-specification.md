# Tenant Specification

Version: 1.0

---

# Definition

Tenant represents isolation boundary.

Not business boundary.

Not organizational boundary.

Security boundary.

---

# Isolation Requirements

Tenant A must never access Tenant B resources.

---

# Isolation Layers

Application Layer

Authorization Layer

Database Layer

Audit Layer

Cache Layer

Messaging Layer

---

# Tenant Context Resolution

Priority Order:

1 JWT Claim

2 Session

3 Host Resolution

4 Request Header

---

# Tenant Membership

User may belong to multiple tenants.

---

# Role Scope

Roles are tenant scoped.

Example:

Tenant A

Manager

Permissions:  
Invoice.Approve

---

Tenant B

Manager

Permissions:  
Invoice.Read

---

Different permissions allowed.

---

# Mandatory Fields

Every aggregate root must contain:

TenantId

---

# Tenant Escape

Severity:

Critical

Must trigger security incident.