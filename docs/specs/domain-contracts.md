# Domain Contracts

---

# Tenant

Purpose:

Represents security isolation boundary.

Attributes:

- TenantId
    
- Name
    
- Status
    
- CreatedAt
    

Rules:

- Must be globally unique.
    
- Cannot be deleted if active users exist.
    

---

# User

Purpose:

Represents human identity.

Attributes:

- UserId
    
- Email
    
- Status
    

Rules:

- User may belong to multiple tenants.
    

---

# TenantMembership

Purpose:

Connects user to tenant.

Attributes:

- UserId
    
- TenantId
    

Rules:

- User can have multiple memberships.
    

---

# Role

Purpose:

Represents organizational responsibility.

Attributes:

- RoleId
    
- TenantId
    
- Name
    
- Description
    

Rules:

- Role names unique per tenant.
    

---

# Permission

Purpose:

Represents capability.

Attributes:

- PermissionId
    
- Code
    
- Category
    

Rules:

- Immutable Code.
    
- Versioned.
    

---

# UserRole

Purpose:

Assign role to user.

Attributes:

- UserId
    
- RoleId
    
- TenantId
    

Rules:

- Tenant scoped.
    

---

# Policy

Purpose:

Represents authorization rules.

Attributes:

- PolicyId
    
- Version
    
- Status
    

Rules:

- Policies immutable after publication.
    

---

# Delegation

Purpose:

Temporary permission transfer.

Attributes:

- FromUser
    
- ToUser
    
- PermissionId
    
- ExpiresAt
    

Rules:

- Must have expiration date.
---
## Warehouse Authorization Contract Example

This example illustrates how a business service requests authorization without embedding authorization logic.

### Scenario
Tenant: `tenant-a`  
Service: `warehouse`  
Role: `Supervisor`

Base permissions may include:

- `invoice.create`
- `invoice.edit`
- `invoice.read`

Additional policy constraints:

- `invoice.create` allowed up to 100 times per month
- `invoice.edit` allowed up to 1 time per day
- `invoice.read` allowed up to 3 times per day per invoice

---

## Authorization Request Contract

Example logical request to the authorization decision engine:
```json
{
  "tenantId": "tenant-a",
  "subjectId": "user-123",
  "service": "warehouse",
  "action": "invoice.edit",
  "resource": {
"type": "invoice",
"id": "inv-456"
  },
  "context": {
"roles": ["Supervisor"],
"dailyEditCount": 0,
"monthlyCreateCount": 42,
"resourceDailyReadCount": 2,
"evaluationTime": "2025-01-15T10:30:00Z",
"tenantTimeZone": "Asia/Tehran"
  }
}
```

The business service supplies the business identifiers and execution context.  
The authorization engine remains responsible for the final decision.

---

## Authorization Response Contract

Example logical response:

```json
{
  "decision": "Allow",
  "policySet": "warehouse-invoice-policy",
  "reasonCodes": [
"role-permitted",
"tenant-matched",
"within-daily-edit-limit"
  ]
}
```
Or, when denied:

```json
{
  "decision": "Deny",
  "policySet": "warehouse-invoice-policy",
  "reasonCodes": [
"daily-edit-limit-exceeded"
  ]
}
```
---

## Contractual Rules

The following rules are mandatory:

- the business service must not interpret role-policy semantics locally
- the business service must not compute the final authorization decision itself
- the business service may provide contextual data needed for evaluation
- the authorization engine returns the final allow/deny result
- usage counters are updated only after successful execution

---

## Post-Success Accounting Contract

After a successful operation, the service must emit an event through the Outbox Pattern so downstream components can:

- increment usage counters
- append audit evidence
- support traceability and analytics

Example event:

```json
{
  "eventType": "warehouse.invoice.edited",
  "tenantId": "tenant-a",
  "subjectId": "user-123",
  "resourceId": "inv-456",
  "occurredAt": "2025-01-15T10:30:02Z",
  "metadata": {
"service": "warehouse",
"action": "invoice.edit"
  }
}
```
This event is not the authorization decision itself.  
It is the accounting signal used after successful completion.
