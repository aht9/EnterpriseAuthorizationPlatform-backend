# specs/ai/coding-rules.md

# Coding Rules

Version: 1.0

Status: Mandatory

---

# General Rules

All code must be production-ready.

All code must be testable.

All code must be observable.

All code must be maintainable.

---

# API Rules

All APIs must be versioned.

Example:

/api/v1/authorization

/api/v1/policies

---

All APIs must support:

- CorrelationId
    
- RequestId
    
- TenantId
    

---

# Domain Rules

Entities must not contain infrastructure concerns.

Entities must protect invariants.

Entities must not expose mutable collections.

Business rules belong inside domain models.

---

# Application Layer Rules

Application layer orchestrates use cases.

Application layer must not contain infrastructure implementations.

Application layer must depend on abstractions.

---

# Infrastructure Rules

Infrastructure depends on Application.

Infrastructure must implement interfaces.

Infrastructure must not contain business rules.

---

# Database Rules

Every aggregate root must contain:

- Id
    
- TenantId
    
- CreatedAt
    
- UpdatedAt
    
- Version
    

Optimistic Concurrency is mandatory.

Soft Delete is mandatory.

---

# Event Rules

All events must be versioned.

All events must contain:

- EventId
    
- CorrelationId
    
- TenantId
    
- OccurredAt
    

---

# Messaging Rules

RabbitMQ only.

Outbox Pattern mandatory.

Publisher Confirm mandatory.

Dead Letter Queue mandatory.

---

# Logging Rules

Structured Logging only.

JSON format only.

No string concatenation logs.

Use contextual logging.

Required Context:

- CorrelationId
    
- TenantId
    
- UserId
    

---

# Authorization Rules

Never authorize based on roles.

Always resolve permissions.

Always evaluate policies.

Always audit decisions.

Never store permissions in JWT.

---

# Caching Rules

Redis is cache only.

Redis is never source of truth.

All cache entries must be versioned.

Event-driven invalidation required.

---

# Testing Rules

Unit Tests mandatory.

Integration Tests mandatory.

Contract Tests mandatory.

Critical authorization flows require end-to-end tests.

Minimum Coverage:

80%

Authorization Domain:

95%