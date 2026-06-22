# OData Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable OData querying directly on existing REST API endpoints (`/api/[controller]`) by returning `IQueryable<T>` and adding the `[EnableQuery]` attribute.

**Architecture:** Modify the `GET` endpoints in `NewsArticleController`, `CategoryController`, `TagController`, and `AccountController` to return EF Core `IQueryable` instead of executed lists, and annotate them with `[EnableQuery]`.

**Tech Stack:** ASP.NET Core 8 Web API, Entity Framework Core, Microsoft.AspNetCore.OData.

## Global Constraints

- Do not change the `[Route("api/[controller]")]` definition.
- Keep the existing `[Authorize]` attributes where they are.
- Run `dotnet build` to verify compilation after each controller update.

---

### Task 1: Update NewsArticleController

**Files:**
- Modify: `API/Controllers/NewsArticleController.cs`

**Interfaces:**
- Consumes: `FUNewsManagementContext`
- Produces: `ActionResult<IQueryable<NewsArticle>>` for `GetAll()` and `GetPublic()`

- [ ] **Step 1: Modify `GetPublic()`**

Replace the current `GetPublic` method to return `IQueryable<NewsArticle>` and add `[EnableQuery]`. We also need to add the `using Microsoft.AspNetCore.OData.Query;` at the top of the file.

```csharp
    [HttpGet("public")]
    [EnableQuery]
    public ActionResult<IQueryable<NewsArticle>> GetPublic()
    {
        return Ok(_context.NewsArticles.Where(n => n.NewsStatus == true));
    }
```

- [ ] **Step 2: Modify `GetAll()`**

Replace the current `GetAll` method to return `IQueryable<NewsArticle>` and add `[EnableQuery]`.

```csharp
    [HttpGet]
    [Authorize]
    [EnableQuery]
    public ActionResult<IQueryable<NewsArticle>> GetAll()
    {
        return Ok(_context.NewsArticles);
    }
```

- [ ] **Step 3: Verify build**

Run: `dotnet build API/API.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add API/Controllers/NewsArticleController.cs
git commit -m "feat: enable OData on NewsArticleController"
```

---

### Task 2: Update CategoryController

**Files:**
- Modify: `API/Controllers/CategoryController.cs`

**Interfaces:**
- Consumes: `FUNewsManagementContext`
- Produces: `ActionResult<IQueryable<Category>>` for `GetAll()`

- [ ] **Step 1: Modify `GetAll()`**

Add `using Microsoft.AspNetCore.OData.Query;` at the top. Replace the current `GetAll` method to return `IQueryable<Category>` and add `[EnableQuery]`.

```csharp
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<Category>> GetAll()
    {
        return Ok(_context.Categories);
    }
```

- [ ] **Step 2: Verify build**

Run: `dotnet build API/API.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add API/Controllers/CategoryController.cs
git commit -m "feat: enable OData on CategoryController"
```

---

### Task 3: Update TagController

**Files:**
- Modify: `API/Controllers/TagController.cs`

**Interfaces:**
- Consumes: `FUNewsManagementContext`
- Produces: `ActionResult<IQueryable<Tag>>` for `GetAll()`

- [ ] **Step 1: Modify `GetAll()`**

Add `using Microsoft.AspNetCore.OData.Query;` at the top. Replace the current `GetAll` method to return `IQueryable<Tag>` and add `[EnableQuery]`.

```csharp
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<Tag>> GetAll()
    {
        return Ok(_context.Tags);
    }
```

- [ ] **Step 2: Verify build**

Run: `dotnet build API/API.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add API/Controllers/TagController.cs
git commit -m "feat: enable OData on TagController"
```

---

### Task 4: Update AccountController

**Files:**
- Modify: `API/Controllers/AccountController.cs`

**Interfaces:**
- Consumes: `FUNewsManagementContext`
- Produces: `ActionResult<IQueryable<SystemAccount>>` for `GetAll()`

- [ ] **Step 1: Modify `GetAll()`**

Add `using Microsoft.AspNetCore.OData.Query;` at the top. Replace the current `GetAll` method to return `IQueryable<SystemAccount>` and add `[EnableQuery]`.

```csharp
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<SystemAccount>> GetAll()
    {
        return Ok(_context.SystemAccounts);
    }
```
*Note: Ensure the `[Authorize(Roles = "Admin")]` attribute at the class level is maintained.*

- [ ] **Step 2: Verify build**

Run: `dotnet build API/API.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add API/Controllers/AccountController.cs
git commit -m "feat: enable OData on AccountController"
```
