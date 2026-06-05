# OPA Architecture

Version: 1.0

---

# Purpose

Externalize authorization rules.

---

# Responsibilities

ABAC

Dynamic Policies

Context Evaluation

Risk Evaluation

---
## Stateful Contextual Evaluation

OPA evaluates policies using the authorization input provided at request time.

For simple RBAC decisions, the input may contain only:

- subject
- tenant
- roles
- action
- resource

For contextual and usage-constrained decisions, the input must additionally include runtime state such as:

- current usage counters
- evaluation windows
- resource-scoped counts
- environment attributes
- service-specific context

OPA itself evaluates policies but is not the primary system of record for mutable usage counters. Therefore, usage-related state must be supplied by platform components before policy evaluation.

---

## Runtime Context Provider

A runtime context provider is responsible for supplying stateful evaluation data such as:

- daily action count per user / tenant / service
- monthly action count per user / tenant / service
- daily resource access count per user / tenant / resource
- time-window boundaries
- any other policy-relevant environmental attributes

This provider may rely on:

- Redis
- a dedicated counter store
- cached effective authorization context
- append-only audit/event streams for reconstruction or analytics

The exact storage and consistency model is implementation-specific, but the provider must remain outside business services.

---

## OPA Input Example

Example authorization input for a Warehouse Service request:
```json
{
  "tenantId": "tenant-a",
  "subjectId": "user-123",
  "roles": ["Supervisor"],
  "service": "warehouse",
  "resource": "invoice",
  "resourceId": "inv-456",
  "action": "edit",
  "context": {
"dailyEditCount": 0,
"monthlyCreateCount": 42,
"resourceDailyReadCount": 2,
"evaluationTime": "2025-01-15T10:30:00Z",
"tenantTimeZone": "Asia/Tehran"
  }
}
```

OPA evaluates this input against externalized policies and returns a centralized allow/deny decision.

---

## Operational Flow

### Pre-Authorization
1. the business service receives the request
2. the authorization request is composed
3. runtime usage context is retrieved
4. the complete input is sent to OPA
5. OPA returns `Allow` or `Deny`

### Post-Success Accounting
If the operation succeeds:

1. the business service writes the result and outbox record in the same transaction
2. an integration event is published
3. usage counters are updated asynchronously or reliably downstream
4. append-only audit evidence is recorded

This separation prevents false quota consumption for failed operations.

---

## Policy Boundary Rule

The following boundary is mandatory:

- OPA and policy definitions decide **whether** access is allowed
- runtime context providers supply **stateful evaluation input**
- business services execute business behavior only after an allow decision
- business services do not own authorization rules or usage policies

---

# Forbidden

Authentication

Session Management

Routing

---

# Input

Subject

Action

Resource

Context

---

# Output

Allow

Deny

Reason

PolicyId


---

## Example Input

```
{  
	"subject":{    
		"department":"Finance"  
	},  
	"action":"Invoice.Approve",  
	"resource":{    
		"amount":5000  
	}
}
```

---

## Example Output

```
{  
	"allow":true
}
```
