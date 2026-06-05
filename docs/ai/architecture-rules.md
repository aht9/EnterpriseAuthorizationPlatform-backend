# specs/ai/architecture-rules.md

# Architecture Rules

Version: 1.0

Status: Mandatory

---

# Rule 1

Authentication and Authorization must remain separate bounded contexts.

---

# Rule 2

Business Services must never implement authorization logic.

Business Services request authorization decisions.

Authorization Service produces decisions.

---

# Rule 3

Kong is an API Gateway.

Kong is not an Authorization Engine.

Kong must never contain business authorization logic.

---

# Rule 4

Policies must be externalized.

OPA evaluates policies.

Services must never embed ABAC logic.

---

# Rule 5

Tenant Isolation is mandatory.

Every request must have tenant context.

Every query must enforce tenant filtering.

Every cache key must contain tenant identifier.

---

# Rule 6

No service may access another service database.

Communication methods:

- API
    
- Events
    

Only.

---

# Rule 7

Every state change must emit a domain event.

Examples:

RoleAssigned

PermissionGranted

PolicyPublished

UserDisabled

---

# Rule 8

Every security decision must be auditable.

Authentication events audited.

Authorization events audited.

Policy evaluations audited.

Delegations audited.

---

# Rule 9

Fail Closed by default.

If authorization cannot be evaluated:

Decision = Deny

unless explicitly approved by architecture review.

---

# Rule 10

All contracts are immutable.

Breaking changes require new versions.

---

# Rule 11

Authorization Platform is language agnostic.

All integrations must be possible from:

- .NET
    
- Go
    
- NodeJS
    
- Python
    
- Java
    

---

# Rule 12

Security takes precedence over convenience.

Performance takes precedence over elegance.

Maintainability takes precedence over cleverness.

Observability takes precedence over assumptions.

Architecture takes precedence over shortcuts.