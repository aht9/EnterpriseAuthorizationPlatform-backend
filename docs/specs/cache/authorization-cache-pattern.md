# Authorization Cache Pattern

Flow

Request

↓

Redis

↓

Hit

↓

Decision

---

Miss

↓

Authorization Service

↓

OPA

↓

Cache

↓

Decision