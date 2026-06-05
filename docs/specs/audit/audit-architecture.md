# Audit Architecture

Version: 1.0

---

# Rule

Every Security Action

must generate audit event.

---

# Audit Categories

Authentication

Authorization

Policy

Role

Permission

Delegation

Tenant

---

# Immutable Audit

Required

---

# Audit Storage

Append Only

---

# Retention

7 Years

Configurable

---

## Audit Record

```
{
	"eventId":"",
	"tenantId":"",
	"userId":"",
	"action":"Invoice.Approve",  
	"resource":"Invoice:55",  
	"decision":"Allow",  
	"timestamp":"..."
}
```

---

