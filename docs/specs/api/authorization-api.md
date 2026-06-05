# Authorization API

Version: 1.0

Base Url

/api/v1/authorization

---

# Check Permission

```
POST /authorize
```

Request:

```
{  "tenantId":"tenant-1",  "userId":"user-1",  "action":"Invoice.Approve",  "resourceId":"invoice-55",  "context":{}}
```

Response:

```
{  "decision":"Allow",  "reason":"Permission Granted",  "policy":"FinancePolicy",  "correlationId":"..."}
```

---

# Bulk Authorize

```
POST /authorize/bulk
```

Request:

```
{  "permissions":[    "Invoice.Read",    "Invoice.Approve",    "Invoice.Delete"  ]}
```

---

Response:

```
{  "Invoice.Read":true,  "Invoice.Approve":true,  "Invoice.Delete":false}
```

---

# Effective Permissions

```
GET /users/{userId}/permissions
```

---

Response

```
{  "permissions":[]}
```

---

# Permission Explain

```
GET /users/{userId}/permissions/{permission}
```

---

Response

```
{  "permission":"Invoice.Approve",  "source":"FinanceManager",  "policy":"AmountPolicy"}
```