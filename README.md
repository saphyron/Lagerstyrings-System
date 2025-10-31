# Lagerstyrings-System — README

**Legend:** 📁 Folder • 🧩 C#-Code • ⚙️ config/json • 🪪 .sln/.csproj • 🧾 README/MD

## Code Structure

```text
📁 Lagerstyrings-System/
├─ 🪪 Lagerstyrings System.slnx
├─ 🧾 ER-Diagram.md
├─ 🧾 LICENSE
├─ 🧾 .gitignore
├─ 📁 Lagerstyrings System/                     (Backend — Minimal API, SQL Server, JWT)
│  ├─ 🪪 Lagerstyrings System.csproj
│  ├─ ⚙️ appsettings.json
│  ├─ ⚙️ appsettings.Development.json
│  ├─ 📁 Properties/
│  │  └─ ⚙️ launchSettings.json                 (http://localhost:5107; https://localhost:7261)
│  ├─ 🧩 Program.cs                             (DI, JWT auth, endpoint mapping)
│  └─ 📁 src/
│     ├─ 📁 Database/
│     │  ├─ 🧩 ISqlConnectionFactory.cs
│     │  └─ 🧩 SqlconnectionFactory.cs          (Microsoft.Data.SqlClient)
│     ├─ 📁 Endpoints/
│     │  ├─ 📁 AuthenticationEndpoints/
│     │  │  ├─ 🧩 AuthEndpoints.cs              (/auth/roles …)
│     │  │  └─ 🧩 UserEndpoints.cs              (/auth/users … + /auth/users/login)
│     │  ├─ 🧩 LogEndpoints.cs
│     │  ├─ 🧩 OrderEndpoints.cs                (/orders …)
│     │  ├─ 🧩 OrderItemEndpoints.cs            (/orderitems …)
│     │  ├─ 🧩 ProductEndpoints.cs              (/products …)
│     │  ├─ 🧩 WarehouseEndpoints.cs            (/warehouses …)
│     │  └─ 🧩 WarehouseProductEndpoints.cs     (/warehouseproducts …)
│     ├─ 📁 Orders/
│     │  ├─ 🧩 IOrder.cs
│     │  ├─ 🧩 Order.cs                         (includes OrderRepository)
│     │  ├─ 🧩 OrderItem.cs
│     │  └─ 🧩 OrderTypeEnum.cs
│     ├─ 📁 Product/
│     │  ├─ 🧩 IProduct.cs
│     │  ├─ 🧩 Product.cs                       (includes ProductRepository)
│     │  └─ 🧩 ProductTypeEnum.cs
│     ├─ 📁 Sku/
│     │  └─ 🧩 WarehouseProduct.cs              (includes WarehouseProductRepository)
│     └─ 📁 Warehouse/
│        ├─ 🧩 IWarehouse.cs
│        └─ 🧩 Warehouse.cs                     (includes WarehouseRepository)
├─ 📁 Lagerstyring Frontend/                    (Frontend — Razor Pages, JWT cookie)
│  ├─ 🪪 Lagerstyring Frontend.csproj
│  ├─ ⚙️ appsettings.json                       (Api:BaseUrl; Jwt settings)
│  ├─ ⚙️ appsettings.Development.json
│  ├─ 📁 Properties/
│  │  └─ ⚙️ launchSettings.json                 (http://localhost:5267; https://localhost:7267)
│  ├─ 🧩 Program.cs                             (Razor Pages, JWT bearer; reads cookie `AuthToken`)
│  ├─ 📁 Pages/
│  │  ├─ 📁 Account/
│  │  │  ├─ 🧩 Login.cshtml
│  │  │  ├─ 🧩 Login.cshtml.cs
│  │  │  ├─ 🧩 Logout.cshtml
│  │  │  └─ 🧩 Logout.cshtml.cs
│  │  ├─ 📁 Admin/
│  │  │  ├─ 🧩 Index.cshtml
│  │  │  ├─ 🧩 Index.cshtml.cs
│  │  │  ├─ 🧩 Users.cshtml
│  │  │  └─ 🧩 Users.cshtml.cs
│  │  ├─ 📁 Order/
│  │  │  ├─ 🧩 CreateOrder.cshtml
│  │  │  └─ 🧩 CreateOrder.cshtml.cs
│  │  ├─ 📁 Warehouse/
│  │  │  ├─ 🧩 Warehouses.cshtml
│  │  │  ├─ 🧩 Warehouses.cshtml.cs
│  │  │  ├─ 🧩 WarehouseView.cshtml
│  │  │  └─ 🧩 WarehouseView.cshtml.cs
│  │  ├─ 📁 Shared/
│  │  │  ├─ 🧩 _Layout.cshtml
│  │  │  └─ 🧩 _ValidationScriptsPartial.cshtml
│  │  └─ 🧩 _ViewImports.cshtml
│  ├─ 📁 User/
│  │  └─ 🧩 User.cs
│  └─ 🧩 GlobalUsings.cs
├─ 📁 SQL Statements/                            (SQL Server scripts)
│  ├─ ⚙️ CreateDatabase.sql
│  ├─ ⚙️ CreateTables.sql
│  └─ ⚙️ InsertIntoTable.sql
├─ 📁 Scripts/
│  ├─ ⚙️ Tests.ps1
│  ├─ ⚙️ httpie-collection-authendpoints.json
│  ├─ ⚙️ httpie-collection-userendpoints.json
│  ├─ ⚙️ httpie-collection-orderendpoints.json
│  ├─ ⚙️ httpie-collection-orderitemendpoint.json
│  ├─ ⚙️ httpie-collection-productendpoints.json
│  ├─ ⚙️ httpie-collection-warehouseendpoints.json
│  └─ ⚙️ httpie-collection-warehouseproductendpoint.json
└─ 📁 tests/                                      (xUnit contract tests)
   ├─ 🪪 Test-First.csproj                        (net8.0)
   └─ 📁 src/
      ├─ 📁 Helpers/
      │  ├─ 🧩 Database.cs
      │  ├─ 🧩 DbUtility.cs
      │  ├─ 🧩 OrdersInvoker.cs
      │  ├─ 🧩 OrdersHttpInvoker.cs
      │  └─ 🧩 OrdersCliInvoker.cs
      └─ 📁 Spectests/
         ├─ 🧩 AuthRolesContractTests.cs
         ├─ 🧩 UserContractTests.cs
         ├─ 🧩 OrdersContractTests.cs
         ├─ 🧩 OrderItemsContractTests.cs
         ├─ 🧩 ProductsContractTests.cs
         ├─ 🧩 WarehouseContractTests.cs
         └─ 🧩 WarehouseProducts.cs
```

## System Requirements

* **.NET SDK:** 9.0 (backend and frontend target `net9.0`).
* **.NET Runtime for tests:** 8.0 (tests project targets `net8.0`).
* **SQL Server:** A local or remote SQL Server instance reachable by the connection string in configuration.
* **PowerShell:** Windows PowerShell 5.1 (for `Scripts\Tests.ps1`).
* **Optional:** HTTPie Desktop/CLI to use the included HTTP collections.

## Configuration

### Backend (`Lagerstyrings System/appsettings*.json`)

* `ConnectionStrings:Default` — SQL Server connection string.
* `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key` — JWT settings used by both API and frontend.

### Frontend (`Lagerstyring Frontend/appsettings*.json`)

* `Api:BaseUrl` — base URL of the backend API (default points to `http://localhost:5107`).
* `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key` — must match the backend.

> Note: The frontend reads the JWT from an **HttpOnly cookie** named **`AuthToken`** (set on successful login). The frontend config includes authentication middleware configured for this cookie.

## Database Setup (SQL Server)

1. Create the database:

   * Run `SQL Statements/CreateDatabase.sql` against your SQL Server.
2. Create schema (drops existing objects in the target DB):

   * Run `SQL Statements/CreateTables.sql`.
3. (Optional) Clear/reseed utility:

   * `SQL Statements/InsertIntoTable.sql` cleans/resets data (no demo users are inserted here).

## How to Build and Launch

### Backend API (Minimal API)

* Working directory: repository root.
* Run:

  ```bash
  dotnet run --project "Lagerstyrings System/Lagerstyrings System.csproj"
  ```
* Default URLs (from `launchSettings.json`):

  * HTTP: `http://localhost:5107`
  * HTTPS: `https://localhost:7261`
* Health route:

  * `GET /` → `{ ok = true, service = "Lagerstyrings System API" }`

### Frontend (Razor Pages)

* Working directory: repository root.
* Ensure `Api:BaseUrl` in `Lagerstyring Frontend/appsettings.json` points to the backend.
* Run:

  ```bash
  dotnet run --project "Lagerstyring Frontend/Lagerstyring Frontend.csproj"
  ```
* Default URLs (from `launchSettings.json`):

  * HTTP: `http://localhost:5267`
  * HTTPS: `https://localhost:7267`
* Entry:

  * Root (`/`) routes to `/Account/Login`. On success, the page stores the JWT in the `AuthToken` cookie.

### Creating a User and Logging In

* The **Users** table stores `Username`, `PasswordClear`, `AuthEnum`, and optional `WarehouseId` (see `CreateTables.sql`).
* The API exposes:

  * `POST /auth/users` to create a user (expects `Username`, `PasswordClear`, `AuthEnum`, `WarehouseId`).
  * `POST /auth/users/login` to obtain a JWT by `Username` and `Password`.
* HTTPie collections in `/Scripts` include ready-made requests for these endpoints.

## API Overview (route groups)

These are registered in `Program.cs` and corresponding `*Endpoints.cs`:

* `/auth/roles` — CRUD for authorization roles (group has `RequireAuthorization()` in code).
* `/auth/users` — CRUD for users (group has `RequireAuthorization()`; selected routes call `.AllowAnonymous()` in code).

  * `/auth/users/login` — anonymous login endpoint (JWT issuance).
* `/orders` — create, read (by id, by user), update, patch status, delete.
* `/orderitems` — create, read by order, read product count within order, update, delete.
* `/warehouses` — CRUD.
* `/products` — CRUD.
* `/warehouseproducts` — CRUD + by-warehouse listing.

> Authorization notes based on code at time of inspection:
>
> * The **AuthRoles** and **Users** route groups call `RequireAuthorization()`; multiple user routes additionally call `AllowAnonymous()`.
> * Other resource groups (**products**, **warehouses**, **orders**, **orderitems**, **warehouseproducts**) do not call `RequireAuthorization()` in their endpoint mapping files.

## Tests

### Test Project

* Location: `tests`
* Framework: xUnit (`Test-First.csproj` targets `net8.0`).
* Contract tests exist for users, roles, orders, order-items, products, warehouses, and warehouse-products.
* Tests rely on environment variables for service URLs and DB connection (see below).

### Environment variables used by tests

* `ConnectionStrings__Default`
* `ORDERS_URL`
* `CATALOG_URL`
* `INVENTORY_URL`
* `PRODUCTS_URL`
* `AUTH_URL`
* `ORDERS_CLI` (optional; used by CLI-based helpers)

### Run tests (standard)

From repository root:

```bash
dotnet test tests/Test-First.csproj
```

Examples with logging:

```bash
dotnet test tests/Test-First.csproj -l "trx;LogFileName=TestResults.trx"
dotnet test tests/Test-First.csproj --filter "FullyQualifiedName~ProductsContractTests"
```

### Run tests via script (`Scripts\Tests.ps1`)

PowerShell 5.1 examples (from repository root):

```powershell
# Default run
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1

# Smoke/health-only pings (quick check)
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 -Smoke

# Override service URLs (if ports differ)
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 `
  -OrdersUrl "http://localhost:5107" `
  -CatalogUrl "http://localhost:5107" `
  -InventoryUrl "http://localhost:5107" `
  -ProductsUrl "http://localhost:5107" `
  -AuthUrl "http://localhost:5107"

# Provide DB connection string for tests (if not present in environment)
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 `
  -ConnectionString "Server=localhost;Database=LSSDb;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;"

# Filter tests (passes through to `dotnet test --filter`)
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 -Filter "FullyQualifiedName~OrdersContractTests"
```

The script sets or echoes the environment variables listed above and orchestrates optional smoke HTTP checks before invoking `dotnet test`.

## HTTPie Collections

* Location: `/Scripts/httpie-collection-*.json`
* Format: HTTPie Desktop export (`meta.schema: https://schema.httpie.io/1.0.0.json`).
* Collections cover: auth roles, users, orders, order-items, products, warehouses, warehouse-products.
* Usage: import into HTTPie Desktop (or compatible tooling) and execute requests against the configured base URL. Some collections include a bearer token in the exported file; replace with a valid token for your environment if required.

## Security Notes (as implemented)

* The database schema includes a `Users` table with `PasswordClear` (plain-text) column (see `CreateTables.sql`).
* Frontend/Backend JWT settings must match; the frontend reads the token from `AuthToken` cookie.
* Endpoint authorization configuration is as described in **API Overview**.

## Commit History

*(From the repository’s git log; messages grouped by commit day.)*

```
Commits on Oct 30, 2025
    Enhance documentation and code clarity across various components
    Add User class to frontend and remove redundant Warehouse constructor
    import Thors Kode
    Add TODO comment for retrieving all orders; update SQL script to include user selection
    Views

Commits on Oct 29, 2025
    Add HTTPie collections for OrderItem, Product, Warehouse, and WarehouseProduct endpoints
    User-view, finally

Commits on Oct 27, 2025
    Refactor seed data script for improved clarity and structure; added comprehensive order and user endpoint collections for HTTPie with authentication.
    Refactored Name for Frontend
    Updateret med Thors Kode
    Implement authentication and authorization features with login and logout pages

Commits on Oct 24, 2025
    Add parameterless constructors to Product, Warehouse, and WarehouseProduct classes
    lavet parameterløs klasse til orderitem og ordertypeenum
    opdaterede program.cs til at virke med thors kode, fixede DTO på order så den virkede med Dapper.
    Hentet thors branch
    Comment Code

Commits on Oct 23, 2025
    fjernet udkommentering af thors kode
    opdaterede sql sætninger, opdateret test script
    Init Test Branch - outcomment thors kode for testning purposes
    small thing
    Fixes to order class

Commits on Oct 22, 2025
    Refactor order and item tables and triggers
    Fix relationship in ER diagram from SALESORDERS to ORDERS
    Refactor ER diagram to consolidate order types
    Adopted project structure

Commits on Oct 21, 2025
    Oprettet Order klasser

Commits on Oct 20, 2025
    Warehouse, SKU, Product classes created
    API Endpoints
    Opdateret ER-Diagram
    User, connectionsettings og sql statements
    Initial Udkast til ER-Diagram
    Init C# Project
    Update copyright holders in LICENSE file
    Initial commit

```

## License

* See `LICENSE` in the repository root.

---

### Notes on exact defaults and ports

* Backend default HTTP port: `5107` (per `Lagerstyrings System/Properties/launchSettings.json`).
* Frontend default HTTP/HTTPS ports: `5267` / `7267` (per `Lagerstyring Frontend/Properties/launchSettings.json`).
* Frontend `Api:BaseUrl` default: `http://localhost:5107`.

### Known cookies

* `AuthToken` — JWT bearer token stored as HttpOnly cookie by the frontend on successful login.

### Known issues

* **Consistent test failure (HTTP 500):** One automated test returns HTTP 500 when run under the test harness; the same scenario succeeds outside the tests. Root cause is under investigation.
* **Auth mismatch (frontend vs. backend):** Problem in Frontend, while frontend routes still require authentication, backend routes does not. If Frontend is not desired, or are fixed, authentication can easily be enabled again by removing ``.AllowAnonymous()`` where needed.
* **Deprecated code present:** Some legacy/deprecated code remains due to time constraints. It does not affect current functionality but should be removed in a future cleanup.
