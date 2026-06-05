# Authorization Specification

Version: 1.0

Status: Approved

---

# Purpose

Provide centralized authorization decisions for all services.

Authorization determines:

Can Subject perform Action on Resource under Context?

---
## Authorization Decision Model

Authorization is evaluated using the following decision model:

Can Subject perform Action on Resource under Context?
Decision = Subject + Action + Resource + Context + Policies

Where:

- **Subject** identifies the actor requesting access
- **Action** identifies the requested operation
- **Resource** identifies the target domain object or capability
- **Context** includes runtime, tenant, environment, and usage-related attributes
- **Policies** define the evaluation rules applied by the policy engine

This model applies to all authorization decisions across the platform.

---

## Supported Authorization Styles

The platform supports a composite authorization model, including:

1. **Tenant-scoped RBAC**
    - base permission assignment through roles
    - tenant-aware role membership and scope isolation

2. **ABAC / Context-aware Authorization**
    - decisions based on request-time attributes
    - environment, tenant, service, subject, and resource context

3. **Usage-based Authorization**
    - decisions constrained by quotas, counters, and time windows
    - examples:
- daily action limits
- monthly action limits
- per-resource access limits

A request may require all three forms to be evaluated together.

--- 

## Usage-Based Authorization

Usage-based authorization applies when a decision depends on prior consumption or operation counts within a defined time window.

Examples:

- allow `invoice.create` only if monthly create count is below 100
- allow `invoice.edit` only if daily edit count is below 1
- allow `invoice.read` only if daily read count for the same invoice is below 3

Usage-based authorization is evaluated as part of **Context** and must not be implemented directly inside business services.

---

## Resource-Scoped Counters

Some usage policies are scoped to a specific resource instance rather than only to subject or tenant.

Examples:

- a user may view `invoice:{id}` at most 3 times per day
- a user may approve a specific request only once
- a user may update a specific object within a limited window

In such cases, the authorization input must include both:

- the target resource identity
- the usage counter values for that resource scope

---

## Pre-Authorization and Post-Success Accounting

Usage-constrained authorization requires a two-phase operational model:

### Phase 1: Pre-Authorization
Before executing the business operation:

- the caller composes the authorization request
- current usage state is retrieved from the runtime context provider
- the policy engine evaluates the request
- the outcome is `Allow` or `Deny`

### Phase 2: Post-Success Accounting
Only after the business operation completes successfully:

- the platform publishes the result using the Outbox Pattern
- usage counters are incremented or updated
- audit evidence is appended

Counters must not be increased for rejected or failed operations.

---

## Design Constraints

The following constraints are mandatory:

- authorization logic must remain externalized
- business services must not contain embedded quota rules
- tenant boundaries must be preserved in all counters and evaluation inputs
- policy evaluation and accounting must be traceable and auditable
- time-window semantics must be explicitly defined
- race conditions and concurrent usage updates must be handled by implementation-specific mechanisms



---

# Authorization Formula

Decision =

Subject  
+  
Action  
+  
Resource  
+  
Context  
+  
Policies

---

# Authorization Layers

Layer 1

Authentication

---

Layer 2

Tenant Validation

---

Layer 3

Role Resolution

---

Layer 4

Permission Resolution

---

Layer 5

Delegation Resolution

---

Layer 6

Policy Evaluation

---

Layer 7

Decision

---

# Decision Types

Allow

Deny

NotApplicable

Indeterminate

---

# Decision Model

Authorization decisions must be deterministic.

Given identical inputs:

Same output must be produced.

---

# Enforcement Points

Gateway

Service SDK

Authorization Service

Policy Engine

Database Layer

---

# Fail Strategy

Sensitive Operations:

Fail Closed

---

Read Operations:

Configurable

Fail Open not recommended.