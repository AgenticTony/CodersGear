# CodersGear

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![Stripe](https://img.shields.io/badge/Stripe-635BFF?style=for-the-badge&logo=stripe&logoColor=white)
![Printify](https://img.shields.io/badge/Printify-API-18A0FB?style=for-the-badge)

**A Full-Stack E-Commerce Platform for Print-on-Demand Merchandise**

[Features](#features) • [Architecture](#architecture) • [Tech Stack](#tech-stack) • [Getting Started](#getting-started)

</div>

---

## Overview

CodersGear is a production-ready e-commerce web application built with ASP.NET Core MVC, designed for selling print-on-demand merchandise. The platform integrates with **Stripe** for secure payment processing and **Printify** for automated order fulfillment, demonstrating enterprise-level architecture patterns and real-world API integrations.

### Key Highlights

- **Full-Stack Development**: Complete MVC application with server-side rendering, RESTful APIs, and modern frontend
- **Payment Integration**: Secure checkout flow with Stripe, including webhook handling for async payment events
- **Third-Party API Integration**: Bi-directional sync with Printify API for product management and order fulfillment
- **Clean Architecture**: Repository pattern, Unit of Work, dependency injection, and separation of concerns
- **Production Considerations**: Environment-specific configuration, comprehensive error handling, and security best practices

---

## Features

### Customer-Facing

| Feature | Description |
|---------|-------------|
| **Product Catalog** | Browse products by category with pagination and filtering |
| **Product Variants** | Select size and color options for each product |
| **Tiered Pricing** | Automatic price breaks at 1-50, 50+, and 100+ units |
| **Shopping Cart** | Persistent cart for guests (session) and authenticated users |
| **Guest Checkout** | Complete purchases without account creation |
| **Secure Payments** | Stripe Checkout with PCI-compliant payment processing |
| **Order Tracking** | View order history and fulfillment status |

### Administrative

| Feature | Description |
|---------|-------------|
| **Product Management** | Full CRUD operations with image uploads |
| **Category Management** | Organize products into categories |
| **Printify Sync** | One-click product import from Printify catalog |
| **Webhook Management** | Configure and monitor Printify webhooks |
| **Background Sync** | Automatic product synchronization service |

---

## Architecture

### System Architecture

```mermaid
flowchart TB
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end

    subgraph Application["Application Layer - ASP.NET Core MVC"]
        Controllers["Controllers"]
        Views["Razor Views"]
        Services["Business Services"]
    end

    subgraph Domain["Domain Layer"]
        Models["Domain Models"]
        ViewModels["View Models"]
    end

    subgraph Data["Data Access Layer"]
        Repository["Repository Pattern"]
        UoW["Unit of Work"]
        DbContext["EF Core DbContext"]
    end

    subgraph External["External Services"]
        Stripe["Stripe API"]
        Printify["Printify API"]
    end

    subgraph Storage["Data Storage"]
        SQLServer[("SQL Server")]
    end

    Browser --> Controllers
    Controllers --> Views
    Controllers --> Services
    Controllers --> UoW
    Services --> Repository
    UoW --> Repository
    Repository --> DbContext
    DbContext --> SQLServer
    Services --> Stripe
    Services --> Printify
```

### Project Structure

```mermaid
flowchart LR
    subgraph Solution["CodersGear Solution"]
        Main["CodersGear<br/>Main Web App"]
        Data["CodersGear.DataAccess<br/>Data & Repositories"]
        Models["CodersGear.Models<br/>Domain Entities"]
        Utility["CodersGear.Utility<br/>Services & Config"]
    end

    Main --> Data
    Main --> Models
    Main --> Utility
    Data --> Models
    Utility --> Models
```

### Database Schema

```mermaid
erDiagram
    ApplicationUser ||--o{ OrderHeader : places
    ApplicationUser ||--o{ ShoppingCart : has
    ApplicationUser {
        string Id PK
        string Name
        string Email
        string PhoneNumber
        string StreetAddress
        string City
        string State
        string PostalCode
        string Country
        int CompanyId
        string StripeCustomerId
    }

    Category ||--o{ Product : contains
    Category {
        int CategoryId PK
        string Name
        int DisplayOrder
    }

    Product ||--o{ ShoppingCart : "added to"
    Product ||--o{ OrderDetail : "ordered in"
    Product {
        int ProductId PK
        string ProductName
        string Description
        string UPC
        int CategoryId FK
        decimal ListPrice
        decimal Price
        decimal Price50
        decimal Price100
        string ImageUrl
        string AdditionalImages
        bool IsPrintifyProduct
        string PrintifyProductId
        string PrintifyVariantData
        bool Visible
    }

    OrderHeader ||--|{ OrderDetail : contains
    OrderHeader {
        int Id PK
        string ApplicationUserId FK
        DateTime OrderDate
        decimal OrderTotal
        string OrderStatus
        string PaymentStatus
        string TrackingNumber
        string PaymentIntentId
        string PrintifyOrderId
        bool SentToPrintify
    }

    OrderDetail {
        int Id PK
        int OrderHeaderId FK
        int ProductId FK
        int Count
        decimal Price
        string Size
        string Color
        string PrintifyVariantId
    }

    ShoppingCart {
        int Id PK
        int ProductId FK
        string ApplicationUserId FK
        string SessionId
        int Count
        string Size
        string Color
        string PrintifyVariantId
    }
```

### Order Processing Flow

```mermaid
sequenceDiagram
    participant User
    participant App as CodersGear
    participant Stripe
    participant Printify

    User->>App: Add items to cart
    User->>App: Proceed to checkout
    App->>App: Validate cart & user info
    App->>Stripe: Create Checkout Session
    Stripe-->>App: Session URL
    App->>User: Redirect to Stripe
    User->>Stripe: Complete payment
    Stripe->>App: Webhook: checkout.session.completed
    App->>App: Create OrderHeader & OrderDetails
    App->>Printify: Create order with line items
    Printify-->>App: Order ID
    App->>App: Update order with PrintifyOrderId
    Printify->>App: Webhook: order:updated
    App->>App: Update order status
    App->>User: Order confirmation email
```

---

## Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Application framework |
| ASP.NET Core MVC | 10.0 | Web framework |
| Entity Framework Core | 10.0 | ORM & database access |
| ASP.NET Core Identity | 10.0 | Authentication & authorization |
| SQL Server | 2022 | Primary database |

### Frontend

| Technology | Purpose |
|------------|---------|
| Razor Views | Server-side rendering |
| Bootstrap 5 | CSS framework |
| jQuery | DOM manipulation & AJAX |
| Bootstrap Icons | Icon library |

### Integrations

| Service | SDK/Method | Purpose |
|---------|------------|---------|
| Stripe | Stripe.NET 50.4.0 | Payment processing |
| Printify | REST API | Print-on-demand fulfillment |

### Development Tools

| Tool | Purpose |
|------|---------|
| Visual Studio 2022 / VS Code | IDE |
| dotnet CLI | Build & run |
| Entity Framework Migrations | Database schema management |

---

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full instance)
- [Stripe Account](https://stripe.com) (for payments)
- [Printify Account](https://printify.com) (for fulfillment)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/AgenticTony/CodersGear.git
   cd CodersGear
   ```

2. **Configure connection string**

   Update `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CodersGearDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Configure API keys**

   Update `appsettings.Development.json`:
   ```json
   {
     "Stripe": {
       "SecretKey": "sk_test_...",
       "PublishableKey": "pk_test_..."
     },
     "Printify": {
       "ApiToken": "your-printify-api-token",
       "ShopId": "your-shop-id"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   cd CodersGear
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**

   Navigate to `https://localhost:5001` or `http://localhost:5000`

---

## Design Patterns

### Repository Pattern with Unit of Work

```mermaid
classDiagram
    class IUnitOfWork {
        <<interface>>
        +ICategoryRepository Categories
        +IProductRepository Products
        +IShoppingCartRepository ShoppingCarts
        +IOrderHeaderRepository OrderHeaders
        +IOrderDetailRepository OrderDetails
        +IApplicationUserRepository ApplicationUsers
        +Save()
    }

    class IRepository~T~ {
        <<interface>>
        +GetAll() IEnumerable~T~
        +GetFirstOrDefault() T
        +Add(entity: T)
        +Update(entity: T)
        +Remove(entity: T)
    }

    class Repository~T~ {
        -ApplicationDbContext _db
        +GetAll() IEnumerable~T~
        +GetFirstOrDefault() T
        +Add(entity: T)
        +Update(entity: T)
        +Remove(entity: T)
    }

    class UnitOfWork {
        -ApplicationDbContext _db
        +ICategoryRepository Categories
        +IProductRepository Products
        +Save()
    }

    IRepository~T~ <|.. Repository~T~
    IUnitOfWork <|.. UnitOfWork
    UnitOfWork --> IRepository~T~
```

### Dependency Injection Configuration

All services are registered in `Program.cs` following the Dependency Inversion Principle:

```csharp
// Repository Pattern
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
services.AddScoped<IEmailSender, EmailSender>();
services.AddScoped<IPrintifyService, PrintifyService>();
services.AddScoped<IPrintifyProductSyncService, PrintifyProductSyncService>();
services.AddScoped<IPrintifyOrderService, PrintifyOrderService>();
services.AddScoped<IWebhookSignatureVerifier, WebhookSignatureVerifier>();

// Background Services
services.AddHostedService<PrintifyBackgroundSyncService>();
```

---

## API Integrations

### Stripe Integration

The application integrates with Stripe for secure payment processing:

- **Checkout Sessions**: Redirect-based payment flow
- **Webhooks**: Real-time payment status updates
- **Customer Management**: Stripe customer ID storage for repeat purchases

### Printify Integration

Full Printify API integration for print-on-demand fulfillment:

| Endpoint | Purpose |
|----------|---------|
| `GET /shops` | List available shops |
| `GET /shops/{id}/products` | Fetch products for sync |
| `POST /orders` | Create fulfillment orders |
| `POST /webhooks` | Register webhook endpoints |

### Webhook Security

Both Stripe and Printify webhooks are verified using HMAC-SHA256 signature validation to prevent replay attacks and ensure request authenticity.

---

## Security Features

| Feature | Implementation |
|---------|---------------|
| **Authentication** | ASP.NET Core Identity with secure password hashing |
| **Authorization** | Role-based access (Customer, Company, Admin, Employee) |
| **CSRF Protection** | Built-in MVC antiforgery tokens |
| **SQL Injection** | Parameterized queries via Entity Framework |
| **XSS Prevention** | Razor automatic HTML encoding |
| **Webhook Verification** | HMAC-SHA256 signature validation |
| **HTTPS Enforcement** | Production redirects to HTTPS |
| **Secrets Management** | Environment-specific configuration |

---

## Project Structure

```
CodersGear/
├── CodersGear/                          # Main web application
│   ├── Areas/
│   │   ├── Admin/                       # Admin area
│   │   │   ├── Controllers/             # Category, Product, Webhook
│   │   │   └── Views/
│   │   ├── Customer/                    # Customer-facing area
│   │   │   ├── Controllers/             # Home, Cart
│   │   │   └── Views/
│   │   └── Identity/                    # ASP.NET Identity pages
│   │       └── Pages/Account/
│   ├── Controllers/                     # Root controllers (Webhooks)
│   ├── Services/                        # Business logic services
│   ├── Views/Shared/                    # Layouts and partials
│   └── wwwroot/                         # Static files
│
├── CodersGear.DataAccess/               # Data access layer
│   ├── Data/
│   │   └── ApplicationDbContext.cs      # EF Core DbContext
│   ├── Migrations/                      # Database migrations
│   └── Repository/                      # Repository implementations
│       └── IRepository/                 # Repository interfaces
│
├── CodersGear.Models/                   # Domain models
│   ├── ApplicationUser.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── ShoppingCart.cs
│   ├── OrderHeader.cs
│   ├── OrderDetail.cs
│   └── ViewModels/
│
└── CodersGear.Utility/                  # Utilities & services
    ├── SD.cs                            # Status constants
    ├── EmailSender.cs
    ├── PrintifyService.cs
    ├── StripeSettings.cs
    └── WebhookSignatureVerifier.cs
```

---

## Database Migrations

The project uses Entity Framework Core migrations for database schema management. Key migrations include:

| Migration | Description |
|-----------|-------------|
| AddCategoryTableDb | Initial categories table |
| AddProductsToDb | Products with pricing tiers |
| addIdentityTables | ASP.NET Identity schema |
| extendIdentityUser | Address fields on user |
| addShoppingcartToDb | Shopping cart table |
| addOrderHeaderAndOrderDetailsToDb | Orders schema |
| addPrintifyApiSetup | Printify integration fields |
| AddVariantFieldsToCartAndOrder | Size/color variants |

---

## License

This project is available for portfolio demonstration purposes.

---

<div align="center">

**Built with .NET 10.0 by [Tony Foran](https://github.com/AgenticTony)**

</div>
