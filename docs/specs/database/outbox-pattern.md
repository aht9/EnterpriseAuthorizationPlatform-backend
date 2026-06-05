# Outbox Pattern

Purpose:

Prevent lost events.

---

Flow

Database Transaction

↓

Business Change

↓

Outbox Record

↓

Commit

↓

Publisher

↓

RabbitMQ