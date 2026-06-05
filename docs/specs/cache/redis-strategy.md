# Redis Strategy

Version: 1.0

---

# Purpose

Reduce authorization latency.

---

# Golden Rule

Redis is optimization.

Redis is not source of truth.

---

# Cache Categories

Role Cache

Permission Cache

Decision Cache

Policy Cache

Session Cache

---

# Cache Keys

tenant:{tenantId}:user:{userId}:roles

tenant:{tenantId}:user:{userId}:permissions

tenant:{tenantId}:decision:{hash}

---

# TTL

Roles

60s

Permissions

60s

Policy

30s

Decision

5s

---

# Cache Invalidation

Event Driven

RabbitMQ

Required

---

# Stale Cache Protection

Version Based Cache

Required

---

# Example

PermissionVersion

RoleVersion

PolicyVersion


---

# مهم‌ترین بخش

## Cache Versioning

---

```
PermissionUpdated

Version 10      
		↓
Version 11
```

---

Redis:

```
tenant:A:user:22:permissions:v10
```

---

بلافاصله:

```
tenant:A:user:22:permissions:v11
```

---

بدون نیاز به Flush.

---

این تکنیک در مقیاس بالا فوق‌العاده مهمه.

---