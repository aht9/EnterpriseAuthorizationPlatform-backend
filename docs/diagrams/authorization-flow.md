

```
Client 
│ 
▼
Kong 
│ 
▼
Service 
│ 
▼
Security SDK 
│ 
▼
Authorization Service 
│ 
▼
Redis 
│
▼
Policy Engine 
│ 
▼
Decision 
│ 
▼
Audit Event 
│ 
▼
RabbitMQ 
│ 
▼
Response
```

---

# Delegation Flow

```
Manager 
│ 
▼
Delegation Request 
│ 
▼
Authorization Service 
│ 
▼
Approval Workflow 
│ 
▼
Delegation Active 
│ 
▼
Audit Event
```

---

# Service To Service Flow

```
Invoice Service 
│ 
▼
Client Credential JWT 
│ 
▼
Customer Service 
│ 
▼
Authorization SDK 
│ 
▼
Decision
```
