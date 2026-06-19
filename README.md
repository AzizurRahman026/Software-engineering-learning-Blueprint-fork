# 🚀 Software Engineer Blueprint — Project Tech Stack Overview

> A **full-stack, AI-powered learning platform** built to demonstrate production-grade software engineering patterns — from clean architecture and real-time communication to multi-LLM orchestration and agentic AI tooling.

---

## 📐 Architecture Overview

This project follows a **strict Layered / Clean Architecture** on the backend, with a clearly separated frontend SPA. Every technology choice was made deliberately to mirror real enterprise standards.

```
┌─────────────────────────────────────────────────────┐
│                   Angular 19 SPA                    │  ← Frontend (Dashboard)
│         NgRx · SignalR Client · Lazy-loaded Routes  │
└───────────────────┬─────────────────────────────────┘
                    │ HTTP / WebSocket (SignalR)
┌───────────────────▼─────────────────────────────────┐
│               ASP.NET Core 10 Web API               │  ← API Layer
│        Controllers · Middleware · CORS · Swagger    │
├─────────────────────────────────────────────────────┤
│               Application Layer (CQRS)              │
│         MediatR Commands · Queries · Handlers       │
│         MCP Tool Definitions · DTOs · Interface     │
├─────────────────────────────────────────────────────┤
│                  Domain Layer                       │
│         Entities · Enums · Interfaces · Excepions   │
├─────────────────────────────────────────────────────┤
│               Infrastructure Layer                  │
│  MongoDB · SignalR Hub · LLM Factory · MCP Service  │
│  MassTransit · Background Services · Repositories   │
└─────────────────────────────────────────────────────┘
```

---

## 🛠️ Key Technologies & Skills Demonstrated

### ⚙️ Backend — ASP.NET Core 10 (.NET 10)

| Technology | What I built |
|---|---|
| **ASP.NET Core 10** | RESTful Web API with controllers, middleware pipeline, environment-specific configs |
| **Clean Architecture** | Strict 4-layer separation: `Domain` → `Application` → `Infrastructure` → `API` |
| **CQRS + MediatR 14** | Every feature is a `Command` or `Query` with a dedicated `Handler` — zero fat controllers |
| **MassTransit 9** | Message bus abstraction with `IMessageBus` interface wrapping MediatR for decoupled event dispatching |
| **MongoDB Driver 3.6** | Custom `DatabaseContext` implementing generic async CRUD, cursor-based pagination, offset pagination, transactions, and connection pool tuning |
| **SignalR** | Real-time push notifications via a `NotificationHub`, integrated with a typed `INotificationService` abstraction |
| **Background Services** | `IHostedService` for startup orchestration (MCP client boot sequence) |
| **Global Exception Middleware** | Centralized error handling across the entire API surface |
| **Swagger / OpenAPI** | Auto-generated, environment-gated API documentation |
| **Docker + Docker Compose** | Both frontend and backend containerised; multi-service `docker-compose.yml` for full-stack local dev |
| **Dependency Injection** | Deep use of DI: singletons, scoped services, options pattern (`IOptions<T>`) throughout |

---

### 🤖 AI & Agentic Systems — LLM Orchestration + MCP

| Technology | What I built |
|---|---|
| **Google Gemini API** (`Google_GenerativeAI 3.6`) | Integrated Gemini as a primary LLM provider via `Microsoft.Extensions.AI` abstraction |
| **Anthropic Claude API** (`Anthropic.SDK 5.10`) | Integrated Claude as a secondary LLM provider, switchable at runtime |
| **LLM Factory Pattern** | `ILlmFactory` + `LlmFactory` — a strategy pattern to create the correct `IChatClient` (Gemini or Claude) based on a `LlmProvider` enum, without changing any consuming code |
| **Microsoft.Extensions.AI 10.4** | Used as the unified abstraction layer (`IChatClient`, `AITool`) across both LLM providers |
| **Model Context Protocol (MCP)** | Implemented a full **in-process MCP server** (`ModelContextProtocol.AspNetCore 1.1`) and connected to it from the **same process** via an `McpClient` over Streamable HTTP transport |
| **MCP Tool Authoring** | Authored `[McpServerTool]`-decorated tools (`TutorialTools`) that the LLM can autonomously invoke to generate structured, level-aware educational blog posts |
| **Agentic Loop** | The backend drives a complete agentic loop: user message → LLM call → tool call decision → MCP tool execution → result injection → final LLM response |
| **In-Memory Chat History** | `IChatHistoryStore` / `InMemoryChatHistoryStore` for threaded, session-aware multi-turn conversations |
| **Chat Thread Management** | Full CRUD for named conversation threads with MongoDB persistence |

---

### 🌐 Frontend — Angular 19

| Technology | What I built |
|---|---|
| **Angular 19** | Standalone components, `inject()` API, modern control flow (`@if`, `@for`) |
| **NgRx (Store + Effects + Selectors)** | Full reactive state management — actions, reducers, effects, selectors — for subjects and course data |
| **Lazy-Loaded Feature Modules** | `loadChildren` route-level code splitting for `Dashboard` and `Courses` feature areas |
| **SignalR Client** (`@microsoft/signalr`) | Real-time notification subscription with automatic reconnect strategy (`[0, 2s, 5s, 10s, 30s]`) and RxJS `Observable` exposure via `Subject` |
| **RxJS** | `Subject`, `Observable` — reactive stream composition throughout the app |
| **Angular Services** | Typed, injectable services (`ChatService`, `SignalrService`, `ConfigService`) that encapsulate all HTTP and WebSocket communication |
| **Angular Router** | Multi-layout routing (`MainLayout`, `CourseLayout`) with nested lazy routes |
| **SCSS** | Per-component scoped styles with shared design tokens |
| **Docker** | Frontend served from an Nginx container built via a `Dockerfile` |

---

### 🗄️ Database & Persistence

| Technology | What I built |
|---|---|
| **MongoDB** | Primary data store for all entities: Subjects, Chapters, Lessons, ChatThreads, ToolCallRecords |
| **Generic Repository Pattern** | Type-safe generic `IDatabaseContext` with methods: `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `DeleteManyAsync`, `GetPagedResponseAsync`, `GetCursorPagedResponseAsync`, `GetItemByConditionAsync` |
| **Cursor-based Pagination** | Production-ready cursor pagination (no `SKIP` performance degradation) backed by MongoDB `_id` ordering |
| **ACID Transactions** | `BeginTransaction` / `CommitTransactionAsync` / `AbortTransactionAsync` with `IClientSessionHandle` |
| **Connection Pool Tuning** | Configured `MaxConnecting`, `MinConnectionPoolSize`, `MaxConnectionPoolSize`, `MaxConnectionLifeTime`, `WaitQueueTimeout`, `WriteConcern.WMajority`, `ReadConcern.Majority` |

---

### 🧱 Design Patterns & Engineering Principles

| Pattern | Where applied |
|---|---|
| **Clean Architecture** | Strict inward dependency flow — `Domain` has zero external dependencies |
| **CQRS** | All writes are `Commands`, all reads are `Queries`, dispatched through MediatR |
| **Repository Pattern** | Generic `IDatabaseContext` abstracts all MongoDB access |
| **Factory Pattern** | `ILlmFactory` creates the right LLM provider without leaking implementation details |
| **Strategy Pattern** | LLM provider selection via `LlmProvider` enum at runtime — open for extension |
| **Options Pattern** | All configs (`MongoSettings`, `GeminiOptions`, `ClaudeOptions`, `McpServerOptions`) bound via `IOptions<T>` |
| **Dependency Inversion** | Every cross-layer dependency is against an interface, never a concrete class |
| **Message Bus Abstraction** | `IMessageBus` wraps MediatR, decoupling feature handlers from the dispatch mechanism |
| **Hosted Service Pattern** | `McpStartupService` bootstraps the MCP client after Kestrel starts, using `IHostedService` lifecycle hooks |

---

### 🚢 DevOps & Deployment — Docker Containers for Easy Deployment

This project is fully containerized, ensuring **"it works on my machine"** translates to **"it works everywhere."** 

| Tool | Usage |
|---|---|
| **Docker** | Individual `Dockerfile` for both frontend (Nginx) and backend (ASP.NET Core) ensuring easy and isolated deployment. |
| **Docker Compose** | Single-command full-stack local environment setup (`docker compose up`) with integrated service networking. |
| **Multi-environment Config** | `appsettings.json` / `appsettings.Development.json` / `appsettings.Production.json` with environment-specific overrides |
| **Render.com** | Deployed containerized frontend at `https://frontend-v1-0-4m1l.onrender.com` |

---

## 📁 Project Structure Snapshot

```
Software-Engineer-Blueprint/
├── Backend/
│   ├── API/               # Controllers, Middleware, Program.cs, Swagger
│   ├── Application/       # CQRS Handlers, MCP Tools, Interfaces, DTOs
│   ├── Domain/            # Entities, Enums, Domain Interfaces
│   ├── Infrastructure/    # MongoDB, SignalR, LLM, MCP, MassTransit, Jobs
│   ├── Contracts/         # Shared message contracts
│   └── Tests/             # xUnit test project
└── Frontend/
    └── Dashboard/         # Angular 19 SPA (NgRx, SignalR, Lazy Routes)
```

---

## 💡 Highlights for Recruiters

- ✅ Built a **multi-provider LLM factory** — swap between Google Gemini and Anthropic Claude at runtime with zero code changes in feature handlers
- ✅ Implemented a **full agentic loop** including in-process MCP server + client + tool discovery + tool invocation
- ✅ Designed a **production-grade MongoDB context** with cursor pagination, connection pooling, and transaction support — not just basic CRUD
- ✅ Used **SignalR** end-to-end: hub on the backend, typed service + RxJS Observable on the Angular frontend, with auto-reconnect
- ✅ Applied **CQRS + MediatR** consistently — every feature is a clean Command or Query, zero business logic in controllers
- ✅ Enforced **Clean Architecture boundaries** — Domain layer has no framework dependencies whatsoever
- ✅ Containerised the entire stack with Docker and deployed to cloud

---

*Built on .NET 10 · Angular 19 · MongoDB · SignalR · Google Gemini · Anthropic Claude · MCP · MassTransit · NgRx · Docker*
