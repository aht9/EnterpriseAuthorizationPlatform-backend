# Role Hierarchy

Version: 1.0

---

# Purpose

Reduce role duplication.

---

# Inheritance

Admin

↓

Manager

↓

Operator

---

Operator Permissions

Invoice.Read

---

Manager Permissions

Invoice.Read

Invoice.Approve

---

Admin Permissions

Invoice.Read

Invoice.Approve

Invoice.Delete

---

# Constraints

Maximum Depth:

5

---

# Circular References

Forbidden.

---

# Resolution

Bottom-up inheritance.

---

# Audit

Hierarchy changes require audit.