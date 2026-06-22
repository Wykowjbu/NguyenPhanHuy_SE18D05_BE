# OData Integration Design

## Context
The project `NguyenPhanHuy_SE18D05_BE` is an ASP.NET Core 8 Web API. It currently has OData configured in `Program.cs` via `Microsoft.AspNetCore.OData`, but the actual API controllers (`NewsArticleController`, `CategoryController`, `AccountController`, `TagController`) are returning standard executed collections (`List<T>`) or anonymous types instead of utilizing OData's querying capabilities.

## Goal
Enable OData querying (`$filter`, `$select`, `$expand`, `$orderby`, `$count`, etc.) directly on the existing REST API endpoints (`/api/[controller]`) without changing the base route to `/odata/`.

## Architecture & Data Flow
We will adopt **Approach 1**, where the `GET` endpoints return the Entity Framework Core `IQueryable<T>` directly.
By returning `IQueryable<T>` and decorating the endpoints with the `[EnableQuery]` attribute, the OData middleware will intercept the request, parse the OData query string, apply the query to the `IQueryable` (which translates to SQL executed by EF Core), and format the response.

### Component Updates

1.  **NewsArticleController**
    *   `GetAll()`: Change return type to `ActionResult<IQueryable<NewsArticle>>`, remove `.Select()` and `.ToListAsync()`, and return `_context.NewsArticles`. Add `[EnableQuery]`.
    *   `GetPublic()`: Change return type to `ActionResult<IQueryable<NewsArticle>>`, remove `.Select()` and `.ToListAsync()`, and return `_context.NewsArticles.Where(n => n.NewsStatus == true)`. Add `[EnableQuery]`.

2.  **CategoryController**
    *   `GetAll()`: Change return type to `ActionResult<IQueryable<Category>>`, remove `.ToListAsync()`, and return `_context.Categories`. Add `[EnableQuery]`.

3.  **TagController**
    *   `GetAll()`: Change return type to `ActionResult<IQueryable<Tag>>`, remove `.ToListAsync()`, return `_context.Tags`. Add `[EnableQuery]`.

4.  **AccountController**
    *   `GetAll()`: Change return type to `ActionResult<IQueryable<SystemAccount>>`, remove `.ToListAsync()`, return `_context.SystemAccounts`. Add `[EnableQuery]`.

## Error Handling & Edge Cases
*   **Circular References:** EF Core navigation properties will cause JSON serialization cycles. The existing `ReferenceHandler.IgnoreCycles` configuration in `Program.cs` handles this.
*   **Security:** Only the `GetPublic` method in `NewsArticleController` is unauthenticated. Other endpoints retain their `[Authorize]` attributes. Returning raw entities exposes the schema, but this is acceptable and standard for this project scope.
*   **Max Expansion Depth / Top:** `Program.cs` already configures `.SetMaxTop(100)` to prevent excessive data retrieval.

## Testing Strategy
After implementation, we will verify the OData integration by running queries locally:
1.  Verify basic retrieval: `GET /api/NewsArticle/public`
2.  Verify filtering: `GET /api/NewsArticle/public?$filter=NewsStatus eq true`
3.  Verify expansion: `GET /api/NewsArticle/public?$expand=Category`
