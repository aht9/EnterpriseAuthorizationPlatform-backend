# Identity API

# Login

```
POST /login
```

Request:

```
{  "username":"",  "password":""}
```

---

Response:

```
{  "accessToken":"",  "refreshToken":"",  "expiresIn":900}
```

---

# Refresh Token

```
POST /refresh-token
```

---

# Revoke Session

```
DELETE /sessions/{id}
```

---

# MFA Verify

```
POST /mfa/verify
```

---

