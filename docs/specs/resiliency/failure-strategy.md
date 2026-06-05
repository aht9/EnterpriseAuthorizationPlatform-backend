# Failure Strategy

---

Authorization Service Down

↓

Redis Decision Cache

↓

Fallback

---

OPA Down

↓

Last Known Policy

↓

Fail Closed

for sensitive operations

---

Redis Down

↓

Database

↓

Rebuild Cache

---

RabbitMQ Down

↓

Outbox Pattern

↓

Retry