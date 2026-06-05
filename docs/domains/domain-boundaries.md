# Bounded Contexts

---

## Identity Context

Responsibilities:

- Login
    
- MFA
    
- Refresh Token
    
- Session Management
    

Owns:

- User
    
- Session
    
- Credentials
    

---

## Authorization Context

Responsibilities:

- Role Management
    
- Permission Management
    
- Group Management
    
- Delegation
    

Owns:

- Role
    
- Permission
    
- Group
    
- Delegation
    

---

## Policy Context

Responsibilities:

- ABAC
    
- Dynamic Rules
    
- Policy Evaluation
    

Owns:

- Policy
    
- Rule
    
- Decision
    

---

## Tenant Context

Responsibilities:

- Tenant Lifecycle
    
- Tenant Isolation
    

Owns:

- Tenant
    

---

## Audit Context

Responsibilities:

- Audit Events
    
- Security Logs
    

Owns:

- AuditRecord
    

---

## Platform Context

Responsibilities:

- SDK
    
- Cache
    
- Eventing
    
- Infrastructure Integration