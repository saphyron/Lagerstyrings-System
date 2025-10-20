# Lagerstyrings System — ER‑diagram & MS-SQL (SQL Server)

---

## ER‑diagram (Mermaid)

```mermaid
erDiagram
    AUTHROLES ||--o{ USERS : assigns

    WAREHOUSES ||--o{ WAREHOUSEPRODUCTS : stocks
    PRODUCTS   ||--o{ WAREHOUSEPRODUCTS : listed

    USERS      ||--o{ SALESORDERS    : placed_by
    USERS      ||--o{ RETURNORDERS   : placed_by
    USERS      ||--o{ TRANSFERORDERS : placed_by

    WAREHOUSES ||--o{ SALESORDERS    : from_or_to
    WAREHOUSES ||--o{ RETURNORDERS   : from_or_to
    WAREHOUSES ||--o{ TRANSFERORDERS : from_or_to

    SALESORDERS    ||--o{ SALESORDERITEMS      : has_items
    RETURNORDERS   ||--o{ RETURNORDERITEMS     : has_items
    TRANSFERORDERS ||--o{ TRANSFERORDERITEMS   : has_items

    PRODUCTS ||--o{ SALESORDERITEMS      : product
    PRODUCTS ||--o{ RETURNORDERITEMS     : product
    PRODUCTS ||--o{ TRANSFERORDERITEMS   : product

    PRODUCTS   ||--o{ INVENTORYLOG : logged
    WAREHOUSES ||--o{ INVENTORYLOG : from_or_to
    USERS      ||--o{ INVENTORYLOG : recorded_by

    AUTHROLES {
        tinyint AuthEnum PK
        nvarchar Name
    }
    USERS {
        int Id PK
        nvarchar Username
        nvarchar PasswordClear
        tinyint AuthEnum FK
    }
    WAREHOUSES {
        int Id PK
        nvarchar Name
    }
    PRODUCTS {
        int Id PK
        nvarchar Name
        nvarchar Type
    }
    WAREHOUSEPRODUCTS {
        int Id PK
        int WarehouseId FK
        int ProductId FK
        int Quantity
    }
    SALESORDERS {
        bigint Id PK
        int FromWarehouseId FK  "nullable"
        int ToWarehouseId   FK  "nullable"
        int UserId          FK
        datetime2 CreatedAt
    }
    SALESORDERITEMS {
        bigint Id PK
        bigint OrderId   FK
        int ProductId    FK
        int ItemCount
    }
    RETURNORDERS {
        bigint Id PK
        int FromWarehouseId FK  "nullable"
        int ToWarehouseId   FK  "nullable"
        int UserId          FK
        datetime2 CreatedAt
    }
    RETURNORDERITEMS {
        bigint Id PK
        bigint OrderId   FK
        int ProductId    FK
        int ItemCount
    }
    TRANSFERORDERS {
        bigint Id PK
        int FromWarehouseId FK  "nullable"
        int ToWarehouseId   FK  "nullable"
        int UserId          FK
        datetime2 CreatedAt
    }
    TRANSFERORDERITEMS {
        bigint Id PK
        bigint OrderId   FK
        int ProductId    FK
        int ItemCount
    }
    INVENTORYLOG {
        bigint Id PK
        datetime2 Timestamp
        char OrderType  "S|R|T"
        int ProductId   FK
        int FromWarehouseId FK  "nullable"
        int ToWarehouseId   FK  "nullable"
        int UserId          FK
        int ItemCount
    }
```