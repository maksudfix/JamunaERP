# JamunaERP

*JamunaERP* is a backend ERP system for a local retail shop that sells Jamuna's products electronics and appliances. It manages sales, installments, stock across multiple locations, dealer purchases, warranty service, expenses, and business reports through a secure REST API built with ASP.NET Core.

## Features

- **Sales and invoicing:** create sales, discounts, void sales, invoice view and printable HTML invoice
- **Sale returns:** return goods and automatically adjust installments and stock
- **Installment plans:** pay installments, track pending and overdue ones, payment receipts
- **Customers:** customer records and customer statements
- **Products and stock:** product catalog with model number, category, buying/selling price and warranty months
- **Multiple stock locations:** Showroom 1, Showroom 2 and Go-down
- **Stock adjustments:** manual corrections with history
- **Purchases from dealers:** purchase orders, receiving goods (updates stock and cost), dealer payments and dealer returns
- **Warranty service:** track claims from Active, Claimed, Sent to Dealer, Repaired to Expired
- **Expenses and employee payments**
- **Reports:** daily sales, profit, customer dues, overdue installments, dealer payables, stock summary
- **Security:** JWT login, role-based access (Admin, Manager, Employee), login rate limiting, CORS control
- **Audit trail:** automatically records who created or changed data
- **Operations:** automatic database backup, health check endpoint (`/health`), Serilog file logging, global error handling

## Technology Stack

### Framework
- ASP.NET Core Web API (.NET 10)

### Architecture
- Clean Architecture (Domain, Application, Infrastructure, WebApi)

### Design Patterns & Validation
- CQRS with MediatR
- Request validation with FluentValidation

### Database & Data Access
- Microsoft SQL Server
- Entity Framework Core (EF Core)

### Authentication & Authorization
- ASP.NET Core Identity
- JWT Bearer Authentication

### API Documentation
- OpenAPI
- Scalar API Reference

### Logging
- Serilog

### Testing
- Unit Testing
- Test Project: `JamunaERP.Application.UnitTests`

## Project Structure

JamunaERP/
├── JamunaERP.Domain/              
├── JamunaERP.Application/          
├── JamunaERP.Infrastructure/       
├── JamunaERP.WebApi/               
├── JamunaERP.Application.UnitTests/
└── JamunaERP.slnx

## Getting Started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server Express
- Visual Studio 2022+ (or the `dotnet` CLI)

### 1. Clone

```bash
git clone https://github.com/maksudfix/JamunaERP.git
cd JamunaERP
```

### 2. Set the database connection

Edit `JamunaERP.WebApi/appsettings.json` (or `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=JamunaERP_Db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Set secrets (never commit these)

```bash
cd JamunaERP.WebApi
dotnet user-secrets set "JwtSettings:Secret" "a-long-random-key-at-least-32-characters"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "YourStrongPassword123!"
```

The first admin user is created automatically from the `SeedAdmin` values.

### 4. Create the database

```bash
dotnet ef database update --project ../JamunaERP.Infrastructure --startup-project .
```

Or set `"Database": { "AutoMigrate": true }` to migrate on startup.

### 5. Run

```bash
dotnet run --project JamunaERP.WebApi
```

In Development, the API documentation (Scalar) opens automatically in your browser. Log in through `POST /api/auth/login`, then use the token as a Bearer token.

### Run the tests

```bash
dotnet test
```

---

## Configuration

`JwtSettings:*`  Token issuer, audience, expiry (secret goes in user-secrets) 
`Cors:AllowedOrigins` Front-end URLs allowed to call the API
`Shop:Name / Address / Phone` Shown on printed invoices
`Backup:AutoEnabled / EveryHours` Automatic backup settings 
`Database:AutoMigrate` Apply migrations on startup 


## Future Plan
### Front End - Angular

### AI-Powered Search
Let staff find information by typing normal sentences instead of using filters.
- **Natural language search:** "Samsung fridges under 50,000 in stock", "customers with overdue installments this month"
- **Smart product search:** match by meaning, not only exact name or model number, and tolerate spelling mistakes (including mixed English/Bangla input)
- **Customer and sale lookup:** find a customer or invoice from partial details such as name, phone or product bought
- **Ask your data:** questions like "What was last week's profit?" answered from the existing report endpoints

### Other ideas
- Low-stock alerts and reorder suggestions
- SMS or WhatsApp reminders for due installments
- Warranty expiry reminders
- Docker support and CI pipeline
  
## Security Notes
- All endpoints require login unless marked public
- Secrets are kept in user-secrets or environment variables, not in the repository
- Do not commit real connection strings, JWT secrets or log files

## Author
Maksud Mubin
Trex Development
https://github.com/maksudfix
