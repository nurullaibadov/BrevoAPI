# BrevoApi - ASP.NET Core 8 Onion Architecture + Brevo

## Hızlı Başlangıç

### 1. appsettings.Development.json düzenle
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BrevoApiDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Brevo": {
    "ApiKey": "YOUR_BREVO_API_KEY"
  }
}
```

### 2. Migration
```bash
dotnet ef migrations add InitialCreate --project src/BrevoApi.Infrastructure --startup-project src/BrevoApi.API
dotnet ef database update --project src/BrevoApi.Infrastructure --startup-project src/BrevoApi.API
```

### 3. Çalıştır
```bash
dotnet run --project src/BrevoApi.API
```
Swagger: http://localhost:5000

## Default Admin
- Email: admin@brevoapi.com
- Şifre: Admin@123456!

## Katman Yapısı
- Domain: Entity, Enum (dependency yok)
- Application: Interface, DTO, Validator, Mapper
- Infrastructure: EF Core, Identity, JWT, Brevo
- API: Controller, Middleware, Program.cs

## Tüm Endpointler
| Method | Route | Auth |
|--------|-------|------|
| POST | /api/v1/auth/register | Public |
| POST | /api/v1/auth/login | Public |
| POST | /api/v1/auth/refresh-token | Public |
| POST | /api/v1/auth/logout | JWT |
| POST | /api/v1/auth/forgot-password | Public |
| POST | /api/v1/auth/reset-password | Public |
| POST | /api/v1/auth/change-password | JWT |
| POST | /api/v1/auth/confirm-email | Public |
| GET | /api/v1/users/me | JWT |
| PUT | /api/v1/users/me | JWT |
| GET | /api/v1/users | Admin |
| DELETE | /api/v1/users/{id} | Admin |
| PATCH | /api/v1/users/{id}/toggle-status | Admin |
| POST | /api/v1/email/send | JWT |
| POST | /api/v1/email/send-template | JWT |
| POST | /api/v1/email/send-bulk | Admin |
| GET | /api/v1/contacts | JWT |
| POST | /api/v1/contacts | JWT |
| PUT | /api/v1/contacts/{id} | JWT |
| DELETE | /api/v1/contacts/{id} | Admin |
| POST | /api/v1/contacts/import | Admin |
| GET | /api/v1/emaillists | JWT |
| POST | /api/v1/emaillists | Admin |
| GET | /api/v1/templates | JWT |
| POST | /api/v1/templates | Admin |
| GET | /api/v1/campaigns | JWT |
| POST | /api/v1/campaigns | Admin |
| POST | /api/v1/campaigns/{id}/send | Admin |
| POST | /api/v1/campaigns/{id}/schedule | Admin |
| POST | /api/v1/campaigns/{id}/pause | Admin |
| POST | /api/v1/campaigns/{id}/cancel | Admin |
| GET | /api/v1/campaigns/{id}/stats | JWT |
| GET | /api/v1/admin/dashboard | Admin |
| POST | /api/v1/webhooks/brevo | Public |
| GET | /health | Public |
