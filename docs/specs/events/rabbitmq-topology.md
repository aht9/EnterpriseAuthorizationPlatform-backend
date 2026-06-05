

```
Exchangesecurity.eventsTypetopic
```

---

Routing Keys

```
user.createduser.disabledrole.createdrole.updatedrole.deletedpermission.updatedpolicy.publishedtenant.createdtenant.disabled
```

---

Queues

```
authorization.cacheaudit.pipelineopa.syncanalytics.pipeline
```

---

# Cache Invalidation

وقتی:

```
RoleUpdated
```

منتشر شود.

---

Queue:

```
authorization.cache
```

باید:

```
User Permission Cache
```

را پاک کند.

---