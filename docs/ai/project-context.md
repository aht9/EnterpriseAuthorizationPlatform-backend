# specs/ai/project-context.md

# Enterprise Authorization Platform

Version: 1.0

Status: Active

---

# Project Mission

Build a centralized Enterprise Authorization Platform capable of serving thousands of tenants, millions of authorization decisions, and heterogeneous microservice ecosystems.

The platform provides:

- Authentication
    
- Authorization
    
- RBAC
    
- ABAC
    
- Policy Evaluation
    
- Delegation
    
- Audit Logging
    
- Tenant Isolation
    

This platform is NOT an Identity Provider only.

This platform is NOT an API Gateway.

This platform is NOT a Business Application.

This platform is a Security Platform.

---

# Technology Stack

Backend

- ASP.NET Core 9
    
- C# 13
    

Data

- PostgreSQL
    
- Redis
    

Messaging

- RabbitMQ
    

Gateway

- Kong
    

Policy Engine

- OPA
    

Observability

- OpenTelemetry
    
- Prometheus
    
- Grafana
    
- Loki
    
- Tempo
    

Infrastructure

- Kubernetes
    
- Helm
    
- GitOps
    

---

# Architecture Style

- Domain Driven Design
    
- Clean Architecture
    
- Vertical Slice Architecture
    
- Event Driven Architecture
    
- Specification Driven Development
    
- Zero Trust Architecture
    

---

# Core Domains

Identity

Responsibilities:

- Login
    
- MFA
    
- Sessions
    
- Refresh Tokens
    

Authorization

Responsibilities:

- Roles
    
- Permissions
    
- Groups
    
- Effective Permissions
    

Policy

Responsibilities:

- ABAC
    
- Dynamic Rules
    
- Risk Evaluation
    

Tenant

Responsibilities:

- Isolation
    
- Membership
    

Audit

Responsibilities:

- Compliance
    
- Security Logging
    

---

# Core Principles

Authentication != Authorization

Role != Permission

Gateway != Authorization Engine

Tenant = Security Boundary

Deny Overrides Allow

Every Decision Must Be Auditable

Every Service Is Untrusted

Policies Must Be Externalized

Authorization Logic Must Be Centralized

---

# Non Functional Requirements

Availability

99.95%

Authorization Latency

P95 < 15ms

Authorization Throughput

100k Requests Per Second

Scalability

Horizontal

Disaster Recovery

Required

---

# AI Agent Responsibilities

Before implementing any feature:

1. Read ADRs
    
2. Read Specifications
    
3. Read Domain Contracts
    
4. Read API Contracts
    
5. Read Event Contracts
    
6. Read Coding Rules
    
7. Read Architecture Rules
    

Never assume missing requirements.

Ask for clarification if requirements are incomplete.

Never invent business rules.

Never bypass specifications.

Specifications are the source of truth.