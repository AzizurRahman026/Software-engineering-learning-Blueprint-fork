# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> Always answer in English (mixed) language.

## Project

A full-stack, AI-powered learning platform. Backend is ASP.NET Core (.NET 10) in strict Clean Architecture; frontend is an Angular SPA (`Frontend/Dashboard`). Core differentiator is an in-process MCP server + client that drives an agentic LLM loop (Gemini or Claude, switchable at runtime).

## Commands

### Backend (run from `Backend/`)
```bash
dotnet build BackendBluePrint.slnx          # build whole solution
dotnet run --project API                    # run the Web API (Swagger at /swagger in Development)
dotnet test                                 # run all xUnit tests
dotnet test --filter "FullyQualifiedName~EmailTests"          # run one test class
dotnet test --filter "FullyQualifiedName~SignupCommandValidatorTests.Validate_Fails_When_Email_Invalid"  # run one test
```
Tests live in `Backend/Tests/` (xUnit). **Unit** tests reference only `Domain` and `Application` — keep them off Infrastructure/API. **Exception:** integration tests under `Backend/Tests/Integration/` legitimately reference `API` (and transitively Infrastructure) to boot the real host via `WebApplicationFactory<Program>`, and require Docker (Testcontainers spins up a throwaway MongoDB). Run integration tests with `dotnet test --filter "FullyQualifiedName~Integration"`.

### Frontend (run from `Frontend/Dashboard/`)
```bash
npm install
npm start            # ng serve — dev server on http://localhost:4200
npm run build        # production build
npm test             # vitest unit tests
```

### Full stack
```bash
docker compose up --build    # frontend → localhost:4200, backend → localhost:5000
```

## Architecture

### Backend layering (dependency flow is inward only)
`API` → `Infrastructure` → `Application` → `Domain`. `Domain` has **zero** external/framework dependencies. Never make `Application` or `Domain` depend on `Infrastructure` or `API`. Cross-layer references are always against an interface, never a concrete class.

- **Domain** — entities (`BaseEntity`, `User`, `BlogPost`, `Chapter`, `Subject`, `ChatThread`…), enums (`LlmProvider`, `NotificationType`), value objects (`Email`), exceptions, repository interfaces.
- **Application** — CQRS feature slices under `Features/<Area>/{Commands,Queries}/<Name>/`, each with a `*Command`/`*Query`, its `*Handler`, and optional `*Validator`. Also holds DTOs, interface definitions (`Common/Interfaces/...`), and MCP tool authoring (`Tools/TutorialTools.cs`).
- **Infrastructure** — concrete implementations: MongoDB (`Persistence/DatabaseContext.cs`, `Repositories/`), SignalR hub + notification service, LLM clients + `LlmFactory`, MCP service, MassTransit `MessageBus`, email (`BrevoEmailSender`), password hashing.
- **API** — thin controllers, middleware, `Program.cs` DI wiring. Service registration is split into `Extensions/ServiceCollectionExtensions.cs` (repos/security) and `Extensions/MasstransitAndMediatRExtensions.cs` (MediatR + validators).

### Request flow (CQRS)
Controllers do **not** call MediatR directly. They inject `IMessageBus` (Infrastructure's `MessageBus`, which wraps MediatR) and call `SendAsync<TCommand, TResponse>(command)`. Every write is a `Command`, every read is a `Query`, each with a dedicated handler. Keep controllers thin — no business logic.

Validation runs automatically: `ValidationBehavior<,>` is a MediatR pipeline behavior that executes every registered FluentValidation `AbstractValidator` before the handler. Validators are auto-discovered from the Application assembly (`AddValidatorsFromAssembly`), so a new `*CommandValidator` is wired up just by existing — no manual registration. On failure it throws `ValidationException`, caught by `GlobalExceptionMiddleware`.

**Adding a feature:** create the folder under `Application/Features/<Area>/{Commands|Queries}/<Name>/`, add the request + handler (+ validator if needed), then a controller action that dispatches it via `IMessageBus`. No DI registration needed for handlers/validators (assembly-scanned).

### AI / agentic subsystem
- `ILlmFactory` (strategy pattern) returns the right `IChatClient` (`GeminiChatClient` or `ClaudeChatClient`) based on the `LlmProvider` enum passed at request time — consuming code never changes.
- The app hosts an **in-process MCP server** (`AddMcpServer().WithToolsFromAssembly(...)`, mapped at `/mcp`) and connects to it from the **same process** via an MCP client. `McpStartupService` (an `IHostedService`) boots the client after Kestrel is listening.
- MCP tools are `[McpServerTool]`-decorated methods in `Application/Tools/TutorialTools.cs`. The chat handler runs the full agentic loop: user message → LLM → tool-call decision → MCP tool execution → result injection → final response.
- Chat identity flows via the `X-User-Id` HTTP header (set by the Angular `user-id.interceptor`), not a token. Chat history is persisted through `IChatHistoryStore` (`MongoChatHistoryStore`).

### Real-time
SignalR `NotificationHub` mapped at `/notifications`; backend pushes via typed `INotificationService` (`SignalRNotificationService`). Angular `SignalrService` subscribes with auto-reconnect and exposes an RxJS `Observable`.

### Persistence
MongoDB via a custom generic `IDatabaseContext` (CRUD, offset + cursor pagination, ACID transactions with `IClientSessionHandle`, connection-pool tuning). The `Email` value object is stored via a custom `EmailSerializer`. Repositories and the DB context are registered as **singletons**.

### Frontend
Angular standalone components (`inject()` API, `@if`/`@for` control flow). State via NgRx (actions/reducers/effects/selectors) for subject/course data. Lazy-loaded feature routes (`Auth`, `Blog`, `Courses`, `dashboard`) under multiple layouts (`MainLayout`, `CourseLayout`). Config (API base URL etc.) comes from `ConfigService` + `environments/`.

## Configuration
API config is bound via the Options pattern (`IOptions<T>`) from `appsettings.json` sections: `McpServer`, `GeminiOptions`, `ClaudeOptions`, `BrevoEmail`, `Auth:PasswordReset`, plus Mongo settings. Provide LLM API keys and the MongoDB connection string before running. CORS allows `http://localhost:4200` and the deployed Render frontend.
