# Authorization ERD

Tenant

 ├── Roles
 │
 ├── Permissions
 │
 ├── Policies
 │
 ├── Groups
 │
 └── Users

Tenant
--------
Id
Name
Status
CreatedAt

Role
--------
Id
TenantId
Name
Description
ParentRoleId
Status
Version

Permission
--------
Id
Code
Category
Version
Status

RolePermission
--------------
RoleId
PermissionId

UserRole
--------
UserId
RoleId
TenantId

Policy
--------
Id
TenantId
Name
Version
Status
PolicyDefinition

Delegation
-----------
Id
TenantId
FromUserId
ToUserId
PermissionId
StartDate
EndDate
Status