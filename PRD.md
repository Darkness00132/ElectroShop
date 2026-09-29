# ElectroShop — Product Requirements Document

## 1. Vision

ElectroShop is a scalable e-commerce backend for digital and consumer electronics retail. It provides catalog, cart, checkout, orders, payments, inventory, procurement, identity, and customer-engagement capabilities as a set of independently evolvable feature slices, built on Clean Architecture and domain-driven design.

> [!NOTE]
> This document describes the product scope and the behavioral decisions the implementation follows. Architecture and contribution rules live in the [README](./README.md) and [CONTRIBUTING](./.github/CONTRIBUTING.md).

## 2. Product Goals

1. Provide a reliable catalog and purchasing experience across bilingual (English/Arabic) content.
2. Prevent inventory overselling during concurrent checkouts.
3. Preserve product and pricing history on completed orders through immutable snapshots.
4. Enforce granular, role-based access for business operations.
5. Keep persistence, storage, payment, and notification integrations replaceable behind application contracts.

## 3. Scope

### 3.1 In Scope (implemented)

| Area | Requirements | Status |
|---|---|---|
| Identity & Access | Registration with email confirmation, JWT login, refresh rotation, revocation, password reset, lockout, roles | ✅ Done |
| Catalog | Categories, brands, products, dynamic attributes, image galleries, discounts | ✅ Done |
| Cart | Persistent cart per customer, availability and quantity validation, checkout summary | ✅ Done |
| Checkout & Orders | Order placement from cart, snapshots, promo codes, stock decrement/restore, order state machine | ✅ Done |
| Payments | Cash on delivery end to end (record, collect, refund) | ✅ Done |
| Inventory | Stock in/out/adjust, reorder levels, auditable transaction log | ✅ Done |
| Procurement | Suppliers, purchase orders, goods receipts feeding inventory | ✅ Done |
| Reviews | Purchase-gated reviews, editing, moderation, public listing | ✅ Done |
| Newsletter | Subscribe, unsubscribe, duplicate prevention | ✅ Done |

### 3.2 Deferred (planned, not implemented)

| Item | Reason |
|---|---|
| Card / wallet payments via `IPaymentGateway` | Cash on delivery is the launch payment method; the gateway contract arrives with card support |
| Two-factor authentication (TOTP) | Large addition to the account flow; lockout + roles cover the current security baseline |
| Address-based shipping calculation | A flat configurable shipping fee is used at checkout |
| Low-stock alert jobs | Reorder levels and low-stock flags exist; alerting jobs are future work |
| Azure production deployment | Local SQL/Redis/storage today; Azure SQL, Blob Storage, and CI/CD are the deployment target |

## 4. Functional Requirements

### 4.1 Identity & Access

- Registration creates the account, assigns the `Customer` role, creates an empty cart, and queues a confirmation email.
- Login returns an access/refresh token pair. Failed attempts count toward lockout (5 failures → 30 minutes); a successful login resets the counter.
- Refresh tokens are single-use and rotated on refresh; password reset revokes all active sessions.
- Web clients authenticate with an access token in the header and a refresh token in a secure `HttpOnly` cookie, protected by antiforgery tokens.
- Emails must be unique. Unknown emails in password-reset requests get the same response as known ones (no account enumeration).

### 4.2 Catalog

- Products, categories, and brands carry English and Arabic names (and descriptions where applicable).
- Products have SKUs (unique), prices, active/inactive status, dynamic key-value attributes (for example RAM, storage, color), and a gallery of 1–5 images.
- Uploaded images are validated (JPG/PNG/WebP, size limits), resized (products 800×800, categories 500×500), and stored as WebP; replaced images are deleted from storage.
- Discounts are percentage (1–100) or fixed amount, with a validity period and a visibility flag. Storefront listings show only active products and only currently valid discounts.

### 4.3 Cart & Checkout

- Each customer has one persistent cart; adding an existing line increases its quantity.
- Add/update operations validate that the product is active and that the total requested quantity does not exceed available stock.
- The checkout summary (`GET /api/cart/checkout`) shows live unit prices, active per-line discounts, available stock, the shipping fee, and computed totals. The same shared calculation is used when placing the order.

### 4.4 Orders

- Placing an order validates the cart, snapshots product name (EN/AR), SKU, unit price, and discount onto each line, applies a promo code if given, adds the shipping fee, decreases stock, and clears the cart — all in one unit of work.
- Promo codes are validated against code existence, validity period, minimum order, and usage limit; usage is counted at checkout.
- Stock changes are guarded by optimistic concurrency: a concurrent checkout that would oversell receives `409 Conflict` instead of succeeding.
- Customers view their orders (summary and detail) and may cancel while the order is `Pending` or `Confirmed`; cancellation restores stock and records the restoring transaction.
- Staff drive the state machine `Pending → Confirmed → Processing → Shipped → Delivered`, or `Cancelled`; delivered orders move to `Refunded` through the payment refund flow.

### 4.5 Payments

- Payments are cash on delivery: staff record one payment per order (duplicate creation is rejected) with a cash-on-delivery attempt.
- Marking a payment as paid is idempotent and records the paid timestamp.
- Refunding requires a paid payment; it marks the payment refunded and the delivered order refunded. The API is idempotent against duplicate callbacks and retries.

### 4.6 Inventory & Procurement

- Every stock change (stock in, stock out, adjustment, checkout, cancellation, goods receipt) is recorded as an inventory transaction with the change, before/after quantities, and provenance (order or goods receipt).
- Inventory tracks quantity on hand and a reorder level; listings expose a low-stock flag.
- Purchase orders follow `Draft → PendingApproval → Approved → PartiallyReceived → Completed` or `Cancelled`; items and costs are managed while in draft.
- Goods receipts are created as drafts against a purchase order; confirming a receipt validates products against the order, records received quantities (never above ordered), increases stock through auditable transactions, and completes the order when fully received.

### 4.7 Reviews

- Only customers with an order containing the product may review it; one review per customer per product.
- Ratings are 1–5 with an optional comment (max 1000 characters).
- Customers edit their own reviews; support agents moderate by removing reviews. Product reviews are publicly browsable, newest first.

### 4.8 Newsletter

- Subscribing stores the email (unique). Subscribing an already-subscribed email is rejected; a previously unsubscribed email resubscribes without creating a duplicate row.
- Unsubscribe is public and idempotent for unknown emails. Administrators can browse subscribers.

### 4.9 Media

- Uploads are validated by content type and size and resized on the server; consumers receive storage keys, not raw files.
- Storage is abstracted behind `IStorageService` (Azure Blob Storage locally configured; local storage possible in development).

## 5. Domain Entities

| Area | Entities |
|---|---|
| Identity | `AppUser`, `AppRole`, `RefreshToken` |
| Catalog | `Category`, `Brand`, `Product`, `ProductImage`, `ProductAttribute`, `Discount` |
| Commerce | `Cart`, `CartItem`, `Order`, `OrderItem`, `PromoCode` |
| Payments | `Payment`, `PaymentAttempt` |
| Operations | `Inventory`, `InventoryTransaction`, `Supplier`, `PurchaseOrder`, `PurchaseOrderItem`, `GoodsReceipt`, `GoodsReceiptItem` |
| Engagement | `Review`, `NewsletterSubscriber` |

The full relational model is documented in [`ecommerce_erd.mmd`](./ecommerce_erd.mmd).

## 6. Roles

| Role | Responsibilities |
|---|---|
| SuperAdmin / Admin | Full administrative control |
| CatalogManager | Categories, brands, products, discounts, and images |
| InventoryManager | Stock, adjustments, reorder levels, and inventory transactions |
| ProcurementManager | Suppliers, purchase orders, and goods receipts |
| SalesManager | Orders, payments, refunds, and promo codes |
| SupportAgent | Review moderation and customer support |
| Customer | Browsing, cart, checkout, cash-on-delivery payments, and reviews |

## 7. Non-Functional Requirements

- **Architecture:** Clean Architecture with inward-pointing dependencies; commands and queries separated via MediatR; shared abstractions only when genuinely shared across slices.
- **Testability:** Domain rules are unit-testable without databases or external services. Tests are behavior-driven — named as business rules and asserting observable outcomes through abstractions, never implementation details.
- **Caching:** Reads are cached with HybridCache (Redis-backed) and evicted through declared keys/tags whenever commands change the data they represent.
- **Security:** Nullable reference types, secure token handling, HttpOnly cookies, antiforgery for cookie flows, lockout, and role policies.
- **Observability:** Structured request logging with slow-request warnings; a global exception handler producing consistent ProblemDetails responses.
- **Deployment target:** Local development with SQL Server, Redis, and local/blob storage; production on Azure SQL Database, Azure Blob Storage, and Azure-hosted application services.

## 8. Success Criteria

- [x] Valid purchases complete without inventory overselling; concurrent checkouts receive a conflict instead of double-spending stock.
- [x] Completed orders retain accurate product name, SKU, and price snapshots.
- [x] Cancelled orders release their reserved stock and record the restoring transaction.
- [x] Payment retries and duplicate submissions do not create duplicate payments.
- [x] Staff can perform only the operations their roles allow.
- [x] Domain and application tests run independently of infrastructure.
