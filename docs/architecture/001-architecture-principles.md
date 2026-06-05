# Architecture Principles

---

## Principle 1

Authentication Is Not Authorization

Authentication answers:

Who are you?

Authorization answers:

What are you allowed to do?

These concerns must be separated.

---

## Principle 2

Role Is Not Permission

Roles are organizational constructs.

Permissions represent capabilities.

Never authorize based on roles directly.

Always resolve permissions.

---

## Principle 3

Tenant Is Security Boundary

Tenant isolation is not a business requirement.

Tenant isolation is a security requirement.

Every entity must support tenant isolation.

---

## Principle 4

Deny Overrides Allow

Evaluation order:

Explicit Deny

before

Explicit Allow

---

## Principle 5

Every Decision Is Auditable

Every access decision must generate an audit event.

---

## Principle 6

Policies Must Be Externalized

Business rules must not be hardcoded inside services.

Policies belong to the Policy Engine.

---

## Principle 7

Zero Trust

No service trusts another service automatically.

Every request must be authenticated.

Every request must be authorized.

---

## Principle 8

Event Driven Synchronization

Cross service synchronization must happen through events.

Never through direct database access.

---

## Principle 9

Gateway Is Not Authorization Engine

Kong validates requests.

Authorization Platform decides permissions.

---

## Principle 10

Business Services Must Remain Thin

Business services should not implement permission evaluation logic.