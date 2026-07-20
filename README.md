# Multi-Tenant SaaS API

ASP.NET Core (.NET 10) Web API with a layered architecture, per-domain tenant
resolution, JWT (HMAC-SHA512) role-based authentication, salted SHA-512
passwords, refresh tokens, and forgot/reset password by email. All data access
uses **stored procedures** via **Dapper**.

## Solution layout

```
WebApplication1            → API / presentation (controllers, middleware, Program.cs)
SaaS.Core                  → contracts only: entities, DTOs (Inbound/Outbound),
                             interfaces, constants, settings, exceptions
SaaS.BLL                   → business logic: AuthService, UserService,
                             JwtTokenService, EmailService, Sha512PasswordHasher
SaaS.DAL                   → Dapper repositories, tenant context/resolver,
                             DbConnectionFactory
sql/                       → database scripts (see below)
```

Dependency direction: `API → BLL → DAL → Core`. Core references nothing else, so
every layer depends only on interfaces defined there.

## How multi-tenancy works

1. The **master database** holds the `Agencies` table (agency + its own DB
   connection string).
2. `TenantResolutionMiddleware` runs after authentication:
   - **Authenticated** requests → resolve the tenant from the `agency_id` JWT claim.
   - **Anonymous** requests (login, forgot/reset) → resolve from the request host
     (or the `X-Tenant-Domain` header locally), matched against `Agencies.DomainUrl`.
3. The resolved connection string is stored in a scoped `ITenantContext`;
   `DbConnectionFactory.CreateTenantConnectionAsync` uses it, so every repository
   call automatically targets the right tenant database.

## Authentication

- **Access token**: JWT signed with HMAC-SHA512, carrying `agency_id`, `user_id`,
  `role_id`, `role_name` (+ standard role claim for `[Authorize(Roles = ...)]`).
- **Refresh token**: opaque CSPRNG value stored per-user in the tenant DB, rotated
  on every refresh.
- **Passwords**: salted SHA-512 (per-user 32-byte salt, constant-time compare).
  > SHA-512 was requested explicitly. For stronger protection against offline
  > brute force, consider migrating to PBKDF2/bcrypt/Argon2 later — only
  > `Sha512PasswordHasher` would change.
- **Roles**: static ids shared across tenants — `SuperAdmin=1, Admin=2,
  Manager=3, User=4` (`SaaS.Core.Constants.RoleConstants`, seeded by the SQL).

`BaseApiController` exposes `CurrentAgencyId`, `CurrentUserId`, `CurrentRoleId`,
etc. from the JWT so controllers never parse claims by hand.

## Setup

1. **Run the SQL scripts** (SSMS or `sqlcmd`):
   - `sql/01_MasterDb_StoredProcedures.sql` → against the **master** database.
   - `sql/02_TenantDb_Schema.sql` and `sql/03_TenantDb_StoredProcedures.sql` →
     against **each** tenant database (e.g. `db43502`).
2. **Configure secrets** — set these outside source control (User Secrets / env):
   - `MasterDb:ConnectionString`
   - `Jwt:SigningKey` (**64+ characters** — required for HMAC-SHA512)
   - `Smtp:*` (host, port, credentials, from-address, `ResetPasswordBaseUrl`)
   Dev values live in `appsettings.Development.json`.
3. `dotnet run --project WebApplication1.csproj`
4. Use `WebApplication1.http` to exercise the flow, starting with
   `POST /api/users/bootstrap-admin` to create the first SuperAdmin.

## Endpoints

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/users/bootstrap-admin` | anon (first user only) |
| POST | `/api/auth/login` | anon |
| POST | `/api/auth/refresh-token` | anon |
| POST | `/api/auth/forgot-password` | anon |
| POST | `/api/auth/reset-password` | anon |
| POST | `/api/auth/change-password` | authenticated |
| POST | `/api/auth/logout` | authenticated |
| GET  | `/api/users/me` | authenticated |
| GET  | `/api/users` | SuperAdmin / Admin |
| POST | `/api/users` | SuperAdmin / Admin |
| GET  | `/api/users/{id}` | SuperAdmin / Admin |

## ⚠️ Security note

Database passwords were shared in plaintext during development. **Rotate the
master and tenant DB passwords**, and keep all connection strings and the JWT
signing key in secrets (never commit them). `appsettings.json` ships with
placeholders for this reason.
