# Usage-Based Authorization Specification

## Purpose

This specification defines authorization rules that depend on prior usage, quotas, counters, rate windows, or resource-scoped access frequency.

It extends the platform authorization model without moving authorization logic into business services.

---

## Scope

This specification applies to cases where authorization depends on:

- number of prior successful actions
- time-based quota windows
- per-resource access frequency
- tenant-aware usage boundaries
- subject-specific or role-specific consumption rules

---

## Core Principle

Usage-based authorization is evaluated as:
Decision = Subject + Action + Resource + Context + Policies

Where `Context` includes usage and quota-related runtime state.

---

## Examples

- max 100 invoice creates per month
- max 1 invoice edit per day
- max 3 reads per day for the same invoice
- max N approvals per user in a rolling window

---

## Architectural Rules

- policies remain externalized in OPA
- business services never encode usage policies directly
- authorization occurs before operation execution
- accounting occurs only after successful completion
- updates must be reliable and auditable
- tenant boundaries are mandatory in all usage state dimensions

---

## Required Dimensions for Counters

Depending on the policy, counters may be keyed by:

- tenant
- subject
- service
- action
- resource type
- resource id
- time window
- policy scope

---

## Consistency Considerations

Implementations must define:

- time zone handling
- daily/monthly window semantics
- concurrency strategy
- retry/idempotency behavior
- failure handling between allow decision and accounting
- atomicity expectations for counter reads/writes

---

## Non-Goals

This specification does not require:

- moving authorization logic into business services
- replacing OPA with imperative authorization code
- using Kong as the authorization engine
- bypassing tenant-aware evaluation boundaries