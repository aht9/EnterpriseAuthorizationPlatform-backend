# specs/ai/domain-glossary.md

# Domain Glossary

Version: 1.0

Status: Canonical

---

# Authentication

The process of verifying identity.

Question:

Who are you?

---

# Authorization

The process of determining allowed actions.

Question:

What are you allowed to do?

---

# Tenant

A security isolation boundary.

Not an organization.

Not a company.

Not a department.

Security boundary.

---

# User

A human identity.

Can belong to multiple tenants.

---

# Service Account

A machine identity.

Used for service-to-service communication.

---

# Role

A collection of responsibilities.

Roles do not grant access directly.

Roles resolve permissions.

---

# Permission

A capability.

Examples:

Invoice.Read

Invoice.Approve

Customer.Update

---

# Effective Permission

Final resolved permission after:

- Roles
    
- Delegations
    
- Policies
    

are evaluated.

---

# Group

A collection of users.

Groups simplify assignments.

Groups do not replace permissions.

---

# Policy

A dynamic rule.

Policies evaluate context.

Policies may allow or deny actions.

---

# RBAC

Role Based Access Control.

User -> Role -> Permission

---

# ABAC

Attribute Based Access Control.

Decision based on:

- Subject
    
- Resource
    
- Action
    
- Context
    

---

# Delegation

Temporary transfer of permissions.

Must have expiration.

Must be auditable.

---

# Decision

Authorization result.

Possible values:

- Allow
    
- Deny
    
- NotApplicable
    
- Indeterminate
    

---

# Explicit Deny

Highest priority decision.

Always overrides Allow.

---

# Audit Record

Immutable security record.

Represents:

Who

Did What

When

Where

Why

Result

---

# CorrelationId

Unique identifier for tracking a request across services.

Mandatory in all distributed operations.

---

# Domain Event

Represents something that happened in the domain.

Examples:

RoleAssigned

UserDisabled

PolicyPublished

---

# Integration Event

An event published for other services.

Versioned.

Immutable.

---

# Outbox Pattern

Reliable event publishing mechanism.

Prevents lost messages.

Mandatory for all critical domain events.

---

# Authorization Decision Engine

Component responsible for producing final authorization decisions.

Inputs:

- User
    
- Tenant
    
- Permissions
    
- Policies
    
- Context
    

Output:

- Allow
    
- Deny
    

Authoritative source of access decisions.