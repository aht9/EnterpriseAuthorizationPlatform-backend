# Authorization Decision Engine

Version: 1.0

---

# Purpose

Produce final authorization decision.

---

# Inputs

Subject

Action

Resource

Context

Policies

---

# Evaluation Order

1 User Status

2 Tenant Status

3 Explicit Deny

4 Tenant Restriction

5 Delegation

6 Role Resolution

7 Permission Resolution

8 Policy Evaluation

9 Final Decision

---

# Decision Priority

Explicit Deny

overrides

Explicit Allow

---

# Decision States

Allow

Deny

NotApplicable

Indeterminate

---

# Caching

Allowed Decisions

5 seconds

---

Denied Decisions

5 seconds

---

# Audit

Every decision logged.

Mandatory.

---

# Correlation

Every decision must include:

RequestId

CorrelationId

TenantId

UserId