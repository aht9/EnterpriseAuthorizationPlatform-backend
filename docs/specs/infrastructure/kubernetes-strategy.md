# Kubernetes Strategy

Version: 1.0

---

# Namespace Layout

security-platform

observability

infrastructure

---

# Deployments

identity-service

authorization-service

policy-service

audit-service

---

# Replicas

Minimum

3

for all critical services.

---

# Autoscaling

HPA Required

CPU

Memory

Request Rate

---

# Pod Disruption Budget

Required

for critical workloads.

```
Authorization Service

minReplicas: 3max

Replicas: 30

targetCPU: 70%
```

---