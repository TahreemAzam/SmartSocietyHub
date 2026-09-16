# Horizon Residencia (SmartSocietyHub)

> **A Better Way to Live.**

A full-stack residential society management platform that centralizes everything a community needs — resident management, complaint tracking, facility booking, billing & payments, and community updates — into a single, role-based web application.

Built with **Clean Architecture (.NET Core)**, **Angular**, and **PostgreSQL**.

---

## 📖 Overview

Instead of managing a residential society through registers, WhatsApp groups, and spreadsheets, Horizon Residencia gives Admins, Residents, and Maintenance Staff one centralized platform to handle day-to-day operations — from reporting a water leak to paying monthly bills to booking the community swimming pool.

## ✨ Features

### 🔐 Authentication & Authorization
- JWT-based authentication via ASP.NET Core Identity
- Role-based access control — **Admin**, **Resident**, **Maintenance Staff**, **Security Guard**
- Custom `ApplicationUser` extending Identity, separated from business-level `Resident` profile data

### 🏘️ Property Management
- Create, update, search, and track properties (house number, block, occupancy status)
- Automatic occupancy sync — assigning/moving a resident updates property status

### 👨‍👩‍👧 Resident Management
- Resident profiles linked to a property and an application user
- Family member records
- Temporary credential generation on account creation

### 🛠️ Complaint & Maintenance Management
- Full complaint lifecycle with a defined state machine:
  `Open → Assigned → InProgress → Resolved → Closed / Reopened`
- Admin assigns staff; residents confirm resolution or reopen unresolved issues

### 🏢 Facility Booking
- Book shared facilities (BBQ area, pool, sports court, kids' play area, rest area)
- Automatic conflict detection with a configurable buffer period between bookings

### 💰 Billing & Payments
- Configurable monthly charges (maintenance, security, water)
- **Snapshot billing** — historical invoices stay accurate even if standard charges change later
- Offline payment workflow: resident uploads payment proof → Admin verifies → bill marked Paid
- PDF invoice generation via QuestPDF

### 📢 Community Updates & Events
- Unified Announcements + Events system
- Event-specific validation (date/time checks)
- Unseen-updates tracking for residents

## 🏗️ Architecture

The backend follows **Clean Architecture** principles, separating concerns into distinct layers:

```
Horizon Residencia
│
├── SmartSocietyHub.API              → Controllers, Program.cs, API configuration
├── SmartSocietyHub.Application       → Service interfaces, business use cases, DTOs
├── SmartSocietyHub.Domain            → Core entities (Property, Resident, Complaint, Bill, etc.)
├── SmartSocietyHub.Infrastructure    → EF Core, PostgreSQL, Identity, PDF generation
├── SmartSocietyHub.Shared            → Shared utilities & models
└── SmartSocietyHub.Frontend          → Angular application
```

**Request flow example:**

```
Angular UI → HTTP Request → API Controller → Authorization Check
    → Application Service → Business Validation → EF Core → PostgreSQL
    → Response → Angular UI Update
```

## 🧰 Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular |
| Backend | ASP.NET Core Web API (Clean Architecture) |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity + JWT |
| PDF Generation | QuestPDF |

## 🚀 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/download/)

### Backend Setup

```bash
cd SmartSocietyHub.API

# Restore dependencies
dotnet restore

# Configure your local secrets (connection string & JWT key)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartSocietyHubDB;Username=postgres;Password=<your-password>"
dotnet user-secrets set "Jwt:Key" "<your-jwt-signing-key>"

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

### Frontend Setup

```bash
cd SmartSocietyHub.Frontend

npm install
ng serve
```

The frontend will run at `http://localhost:4200` and connect to the API at the URL configured in `src/environments/environment.development.ts`.

## 👥 Roles

| Role | Access |
|---|---|
| **Admin** | Properties, Residents, Complaints, Staff, Facilities, Bookings, Billing, Community Updates |
| **Resident** | Dashboard, Profile, Complaints, Facility Bookings, Bills, Community Updates |
| **Maintenance Staff** | Dashboard, Assigned Complaints, Complaint Progress |
