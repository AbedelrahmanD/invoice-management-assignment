# Invoice Management — .NET Senior Developer Test

A Blazor Server application for managing Invoices and their Line Items, built on .NET 10 with EF Core and SQLite.

---

## Tech Stack

- **.NET 10** / C#
- **Blazor Server** (Interactive Server render mode, no prerendering)
- **Entity Framework Core** + **SQLite** (file-based, zero-setup database)
- **Bootstrap 5** + Bootstrap Icons for styling

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- No database server, Docker, or external service required — SQLite is a single file created and migrated automatically on first run.

---

## How to Run

```bash
git clone https://github.com/AbedelrahmanD/invoice-management-assignment
cd InvoiceManagement
dotnet run
```

Open the URL shown in the console (e.g. `https://localhost:7114`).

On first launch, the app automatically:

1. Creates the SQLite database file (`Db/invoices.db`) if it doesn't exist.
2. Applies all pending EF Core migrations.

No manual migration commands or seed scripts required — clone and run.

---

## Editing Invoices

1. From the invoice list (`/`), click **New** to create an invoice, or the pencil icon on any row to edit it.
2. Fill in **Customer Name** and **Date**.
3. Click **Add Line** to add a line item — enter **Name**, **Quantity**, and **Price**. The line total and invoice total recalculate live as you type.
4. Click **Create** / **Update** to save. On success you're redirected to the list with a toast confirmation.
5. On the edit screen, **Delete** removes the invoice and all its line items (confirmation prompt first, cascade delete in the database).

---

## Design Decisions

**Domain model (6 fields max, per assignment constraints)**

- `Invoice`: `Id`, `Number`, `CustomerName`, `Date`, `TotalAmount`, `Items` (navigation collection, not a scalar field).
- `InvoiceItem`: `Id`, `Name`, `Quantity`, `Price`, `InvoiceId`, plus a `[NotMapped]` calculated `LineTotal` (`Quantity * Price`) — not persisted, so it doesn't count against the scalar field budget.
- No back-reference navigation (`InvoiceItem.Invoice`) — kept unidirectional to avoid circular references during Blazor/JSON serialization and to stay within the field limit.

**SQLite, file-based** Satisfies "runnable immediately after cloning" — the whole database is one file, created and migrated automatically via `Database.Migrate()` on startup. No external service, no manual setup step for whoever clones the repo.

**`IDbContextFactory<AppDbContext>` instead of a directly-injected `DbContext`** Blazor Server keeps one long-lived circuit per user, and a plain `AddDbContext` registration (scoped to the circuit) can throw "a second operation was started on this context" if multiple components/operations touch it concurrently. Using `IDbContextFactory` and creating a short-lived context per method call in `InvoiceService` avoids that entirely and is the recommended pattern for Blazor Server + EF Core.

**Service layer behind an interface (`IInvoiceService`)** Keeps `List.razor` / `Form.razor` decoupled from EF Core — they depend only on the interface, injected via DI. `AppDbContext` and `DbContextFactory` never appear in a `.razor` file directly.

**Transactions on write operations** `UpdateAsync` wraps invoice + line-item changes in an explicit transaction with rollback on failure, so a partial save (e.g. invoice updated but items were not deleted) can't happen.

**Server-computed totals, never client-entered** `TotalAmount` is recalculated from `Items.Sum(...)` inside the service on every save — the form only ever *displays* a running total, it never lets the user set it directly, so it can't drift out of sync with the actual line items.

**Auto-incrementing invoice number, generated server-side** `Number` is computed as `MAX(Number) + 1` (starting at 101) inside `InvoiceService`, on save, not from user input. A unique index on `Number` in `AppDbContext` guards against a race between two concurrent creates; a collision throws `DbUpdateException`, which is caught and surfaced as a generic error to the user. Given the scope of this assignment, this is treated as an acceptable trade-off rather than adding full optimistic-concurrency handling.

**Flash messages via a scoped service** `FlashMessageService` is registered `Scoped`, which in Blazor Server means one instance per circuit (per browser tab/session) — exactly the lifetime needed for a "set on this page, read on the next page load" pattern without leaking state between different users.

**Validation**

- `[Required]`, `[Range]` DataAnnotations on the models, enforced via `<DataAnnotationsValidator />` in both forms.
- `InputNumber` components use `ParsingErrorMessage` to give a clean message ("Must be a number") when non-numeric text is typed, since that failure happens during type parsing, before DataAnnotations ever run.
- A submit is blocked entirely if `Items` is empty (`InvoiceService.SaveAsync`/`UpdateAsync` throw `InvalidOperationException`, surfaced as an inline alert).

---

## Manual Test Coverage

The following cases were verified during development:

| Case | Result |
| --- | --- |
| Submit with all fields empty | Blocked — required-field validation messages shown |
| Customer name = whitespace only | Blocked — treated as empty by `[Required]` |
| Valid customer/date, zero line items | Blocked — `InvalidOperationException` surfaced as an alert |
| Negative quantity / negative price | Blocked — `[Range]` validation |
| Quantity or price = `0` | Blocked — `[Range(1, ...)]` / `[Range(0.01, ...)]` require a positive value |
| Non-numeric input (e.g. `"e"`) in Quantity/Price | Blocked — `ParsingErrorMessage` shown before DataAnnotations even run |
| Line item fields left empty | Blocked — `[Required]` on `Name`, `[Range]` on `Quantity`/`Price` |
| Valid invoice, multiple line items | Succeeds — total calculated correctly, redirects with success toast |
| Edit existing invoice | Loads correctly with existing line items populated |
| Delete invoice | Confirmation prompt, cascades to remove line items |