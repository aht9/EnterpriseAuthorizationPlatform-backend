# ABAC Specification

Version: 1.0

---

# Purpose

Support context-aware authorization.

---

# Evaluation Model

Subject

Action

Resource

Environment

---

# Subject Attributes

UserId

Department

Country

Region

EmploymentType

ClearanceLevel

---

# Resource Attributes

ResourceId

ResourceOwner

TenantId

Status

Classification

Amount

---

## Environment and Runtime Attributes

ABAC evaluation may use runtime attributes collected at decision time.

These attributes may include:

- request timestamp
- tenant id
- service name
- client application
- environment classification
- network origin
- device or trust context
- usage counters
- quota windows
- resource-scoped access counts

These attributes are part of **Context** and may influence authorization outcomes.

---

## Usage-Related Context Attributes

The following are examples of usage-related context attributes that may be supplied to the policy engine:

- `dailyEditCount`
- `monthlyCreateCount`
- `resourceDailyReadCount`
- `quotaWindowStart`
- `quotaWindowEnd`
- `evaluationTime`
- `serviceName`
- `tenantTimeZone`

These attributes are evaluated as runtime input and are not hard-coded as business service logic.

---

## Example ABAC + Usage Constraints

Example policy semantics:

- allow `invoice.create` when:
    - subject has base permission
    - tenant matches
    - service context is valid
    - `monthlyCreateCount < 100`

- allow `invoice.edit` when:
    - subject has base permission
    - tenant matches
    - `dailyEditCount < 1`

- allow `invoice.read` when:
    - subject has base permission
    - tenant matches
    - resource id is present
    - `resourceDailyReadCount < 3`

This shows that runtime usage information is modeled as contextual attributes in the ABAC layer.

---

# Policy Categories

Access Policy

Tenant Policy

Financial Policy

Compliance Policy

Risk Policy

Operational Policy

---

# Policy Ownership

Policies must be owned by domain teams.

Platform team must not own business policies.

---

# Evaluation Engine

OPA

Required.