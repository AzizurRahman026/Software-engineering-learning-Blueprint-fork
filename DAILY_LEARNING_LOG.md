# Daily Learning Log

> Tracks additions to the Software Engineering Learning Blueprint project, day by day.

## 2026-07-06 — MongoDB explain plans, index startup initializer, and resilient LLM client

**New additions:**
- `Backend/Infrastructure/Persistence/MongoIndexInitializer.cs` — An `IHostedService` that runs `CreateOneAsync` for hot-path indexes at startup. Because `CreateOneAsync` is idempotent, running it on every boot is safe and ensures indexes are present in every environment (local, Docker, CI's Testcontainers run) without manual `mongosh` intervention. Failure is logged but never thrown, so a Mongo startup race can't kill the app.
- `Backend/Infrastructure/Persistence/Indexing/` (5 files: `IMongoIndexConfiguration.cs`, `MongoIndexConfiguration.cs`, `BlogPostIndexes.cs`, `BlogCommentIndexes.cs`, `BlogLikeIndexes.cs`) — Separates index declaration into its own folder. `IMongoIndexConfiguration` is a small interface that each index class implements, keeping the initializer open to new indexes without modification (Open/Closed Principle applied to schema maintenance).
- `Backend/Infrastructure/Llm/ResilientChatClient.cs` — A decorator around `IChatClient` that adds retry/resilience behaviour (Polly or manual) without touching the underlying Gemini or Claude clients. Demonstrates the Decorator pattern: the consuming `LlmFactory` keeps returning the same interface; the resilience wrapper is composed around it transparently.
- `Frontend/Dashboard/src/app/Core/Models/problem-details.ts` — TypeScript interface matching ASP.NET Core's RFC 7807 `ProblemDetails` response shape. Placing it in `Core/Models` makes it available across all Angular features that handle HTTP errors, without duplicating the type definition per feature.
- `docs/sessions/2026-07-06-day-23.md` — Day 23 session notes covering MongoDB `explain("executionStats")`, reading COLLSCAN vs IXSCAN output, the `totalDocsExamined : nReturned` ratio heuristic, and the production failure mode (silent degradation → 100 MB sort-memory hard error).
- `Playground/LoggerFactoryDemo/` — New empty playground directory, likely scaffolded for the next session's `ILoggerFactory` / structured-logging experiments.

**Concepts reinforced today:**
- MongoDB explain plans — `explain("executionStats")` is how you verify a query's execution strategy; COLLSCAN with `hasSortStage: true` signals a missing index, while IXSCAN with `totalDocsExamined ≈ nReturned` confirms the index is used
- Startup index management — declaring indexes as code (`IHostedService`) rather than in a setup script makes schema changes reproducible and reviewable in version history
- Open/Closed Principle in schema management — a new `IMongoIndexConfiguration` implementation adds an index without touching the initializer loop
- Decorator pattern for cross-cutting concerns — `ResilientChatClient` wraps `IChatClient` to add retry logic without modifying either concrete LLM client
- RFC 7807 ProblemDetails on the frontend — matching the backend error contract with a typed TypeScript interface eliminates guesswork in error-handling code
- Cache vs index — a cache hides a slow query; an index fixes it; both are needed because cache misses, cold starts, and eviction storms still pay the full query cost

---

## 2026-07-02 — Domain-event dispatch moved to the persistence boundary + LLM failure boundary added

**New additions:**
- `Backend/Domain/Common/AggregateRoot.cs` — New base class (extends `BaseEntity`) that is the *only* place domain events can be raised (`RaiseDomainEvent`, `HasDomainEvents`, `GetDomainEvents`, `ClearDomainEvents`). Splitting this out of `BaseEntity` makes event-raising an opt-in capability: plain entities (`BlogLike`, `Chapter`, ...) simply don't inherit it, so they can never trigger dispatch — enforced at compile time, not by convention.
- `Backend/Application/Common/Events/IDomainEventDispatcher.cs` — New interface (`DispatchAsync(AggregateRoot, ...)`) owned by Application, implemented by Infrastructure. Its signature takes `AggregateRoot`, not `BaseEntity`, so the type system itself excludes non-aggregate entities from ever reaching the dispatcher.
- `Backend/Infrastructure/Services/DomainEventDispatcher.cs` — MediatR-backed implementation, now invoked by `Repository` (`Backend/Infrastructure/Repositories/Base/Repository.cs`) right after a successful `AddAsync`/`UpdateAsync`/`DeleteAsync`, instead of from inside `SignupCommandHandler`. Clears the aggregate's event buffer *before* publishing (clear-then-publish, preventing re-entrant double-fires), resolves `IPublisher` from a fresh DI scope via `IServiceScopeFactory`, and catches per-event so one failing handler can't block the rest or fail the already-committed write.
- `Backend/Domain/Exceptions/LlmUnavailableException.cs` — New domain exception raised when an LLM provider call fails (network/auth/quota) or the agentic tool-call loop in `SendChatCommandHandler` exceeds its new `MaxToolIterations` safety cap. Mapped to HTTP 502 by `GlobalExceptionMiddleware`, giving callers a distinct, actionable status instead of a generic 500.
- `Backend/Tests/Application/Features/Auth/SignupDomainEventPublishTests.cs` — Replaces yesterday's `SignupDomainEventDispatchTests.cs` now that dispatch left the handler. Pins the handler's narrower contract: it must RAISE (via `User.Register`) and PERSIST only — the `User` aggregate handed to `IUserRepository.AddAsync` must still be carrying its `UserRegisteredEvent` when the handler returns, since a fake repository (unlike the real `Repository`) never dispatches. Actual dispatch behaviour is left to integration tests.

**Concepts reinforced today:**
- Persistence-boundary event dispatch — moving "drain and publish" out of the command handler and into the Repository means every write path (not just Signup) gets domain-event support for free, and the handler's job shrinks to raise + persist
- Type-driven invariants — `AggregateRoot` vs `BaseEntity` and `IDomainEventDispatcher(AggregateRoot ...)` use the type system, not runtime checks, to guarantee only event-raising entities can ever be dispatched
- Clear-then-publish — still enforced, now at the new dispatch site, to keep side-effects at-most-once
- Failure isolation — a throwing `INotificationHandler` is caught per-event and logged, never allowed to fail an operation whose write already committed (this is at-most-once delivery; an outbox is the noted future fix for at-least-once)
- Fail-fast with typed exceptions — `LlmUnavailableException` turns two failure modes (provider error, runaway agent loop) into one exception type mapped to a single meaningful HTTP status, instead of leaking a raw 500
- Contract testing after a refactor — when responsibility moves between layers, the unit test for the layer that lost responsibility should shrink to match, and the new responsibility gets its own (here: integration-level) test

---

## 2026-07-01 — Domain event handler refactored + event dispatch test suite added

**New additions:**
- `Backend/Application/Features/Auth/Events/DomainEventHandler.cs` — Replaces `UserRegisteredEventHandler.cs` with a generalised handler that subscribes to the non-generic `DomainEventNotification` and dispatches on the concrete domain event type via a `switch`. This "single handler, many events" shape is the Open/Closed Principle in action: each new domain event type is a new `case`, not a new handler class, while still keeping side-effects out of the command handler.
- `Backend/Tests/Application/Features/Auth/SignupDomainEventDispatchTests.cs` — Three-scenario unit test suite that proves the full domain-event lifecycle: (1) a successful signup publishes exactly one `DomainEventNotification` wrapping a `UserRegisteredEvent` with the correct user data; (2) after dispatch the aggregate's internal event buffer is drained (clear-then-publish), so re-saving the same `User` entity would not re-fire side-effects; (3) a failed repository save publishes nothing, ensuring side-effects are strictly conditional on a committed write. Uses a hand-rolled `SpyPublisher` (implements `IPublisher`) to intercept MediatR notifications without a real bus.
- `Frontend/Dashboard/src/app/Shared/Components/sidebar-component/` (4 files) — Angular standalone sidebar component with spec. Demonstrates the Shared module pattern: reusable UI building blocks extracted into a `Shared/Components` folder so any feature module can import them without circular dependencies.
- `Frontend/Dashboard/src/app/Shared/Models/notification.model.ts` — TypeScript interface representing the notification payload. Placing domain models in `Shared/Models` (rather than inside a single feature) makes the type available across all features that deal with notifications (e.g. SignalR subscriber and any UI component that renders notifications).
- `Frontend/Dashboard/src/environments/` (`environment.development.ts`, `environment.production.ts`) — Angular environment files that hold build-time configuration (API base URL, feature flags). The Angular CLI swaps the correct file at build time, mirroring ASP.NET Core's `appsettings.{Environment}.json` layered-config pattern on the frontend.

**Concepts reinforced today:**
- Open/Closed Principle — `DomainEventHandler` can grow with new `case` branches as the domain adds events, without modifying the dispatch mechanism or the command handler
- Clear-then-publish — domain events are collected on the aggregate, dispatched after a successful write, then cleared; ensures side-effects never fire twice for the same write
- Spy test double — a minimal `IPublisher` implementation that records published notifications replaces a full MediatR bus, keeping the unit test fast and assertion-precise
- Test isolation for event-driven code — testing that side-effects do NOT fire on failure is as important as testing that they do fire on success
- Angular environment files — build-time configuration swapping is the frontend equivalent of environment-specific config files; keeps environment-specific values out of source code
- Shared module pattern — extracting reusable components and models to `Shared/` avoids feature coupling and is a prerequisite for lazy-loaded Angular modules

---

## 2026-06-30 — Domain events pattern introduced (IDomainEvent, UserRegisteredEvent, MediatR adapter)

**New additions:**
- `Backend/Domain/Common/IDomainEvent.cs` — A framework-free marker interface for domain events, carrying only `OccurredOnUtc`. Lives entirely in Domain with zero external dependencies, so the Domain layer never takes a reference to MediatR or any infrastructure concern.
- `Backend/Domain/Events/UserRegisteredEvent.cs` — A record that carries the facts of a successful user registration (UserId, Username, Email, OccurredOnUtc). Raised inside `User.Register` via `BaseEntity.RaiseDomainEvent()`, decoupling the signup side-effects from the command handler that creates the user.
- `Backend/Application/Common/Events/DomainEventNotification.cs` — A generic MediatR `INotification` wrapper (`DomainEventNotification<TDomainEvent>`) that adapts any `IDomainEvent` into the MediatR pipeline. This is the Anti-Corruption Layer between the framework-free Domain and the MediatR-aware Application layer.
- `Backend/Application/Features/Auth/Events/UserRegisteredEventHandler.cs` — A `INotificationHandler` that reacts to `DomainEventNotification<UserRegisteredEvent>`. Demonstrates the Observer / Pub-Sub pattern: the `SignupCommandHandler` raises the event and never knows who handles it; multiple handlers can be added for the same event without touching the handler.

**Concepts reinforced today:**
- Domain Events pattern — entities raise events that describe what happened; side-effects are handled outside the aggregate, keeping business logic cohesive and side-effect-free
- Anti-Corruption Layer — `DomainEventNotification<T>` wraps a pure domain concept in the infrastructure (MediatR) vocabulary so the Domain layer stays dependency-free
- Open/Closed Principle — adding a new side-effect (e.g. send welcome email) means adding a new `INotificationHandler`, never modifying the existing signup handler
- Clean Architecture dependency rule — the `IDomainEvent` marker lives in Domain; the MediatR adapter lives in Application; neither ever references infrastructure types
- Observer / Pub-Sub via MediatR — `INotificationHandler<T>` is MediatR's built-in publish/subscribe mechanism; dispatching a notification fans out to all registered handlers automatically

---

## 2026-06-29 — Redis cache-aside pattern and correlation ID integration tests added

**New additions:**
- `Backend/Application/Common/Interfaces/Services/ICacheService.cs` — A thin caching abstraction exposing `GetAsync`, `SetAsync`, and `RemoveAsync`. The Application layer talks only to this interface, keeping Redis and `IDistributedCache` types invisible to handlers; the concrete implementation can be swapped (e.g. in-memory for tests, Redis for production) without touching any feature code.
- `Backend/Application/Features/Blog/BlogCacheKeys.cs` — A static class that is the single source of truth for blog cache key strings. By centralising key formatting in one place, both the read path (which populates the cache) and every write path (which must invalidate it) are guaranteed to use identical keys, preventing stale-cache bugs caused by key drift.
- `Backend/Infrastructure/Services/RedisCacheService.cs` — Concrete `ICacheService` implementation backed by `IDistributedCache` (Redis in production, in-memory as fallback). Serialises values to UTF-8 JSON using `System.Text.Json`, keeping the cache payload technology-agnostic; the Application layer never sees Redis types.
- `Backend/API/appsettings.Development.json` — Development-environment overrides for app configuration (local Redis connection string, dev-specific API keys, etc.). Demonstrates the ASP.NET Core environment-layered configuration system: `appsettings.json` sets safe defaults, `appsettings.Development.json` overrides them locally without affecting production.
- `Backend/Tests/Application/Features/Blog/BlogCacheAsideTests.cs` — Unit tests proving the cache-aside pattern works correctly for the Blog feature: a cache hit returns the cached value and never touches the repository; a cache miss reads from MongoDB and populates the cache; write commands (`AddComment`) evict the exact cache key used by the read path. Uses a hand-rolled `SpyCacheService` test double that records `SetKeys` and `RemovedKeys` for precise assertions.
- `Backend/Tests/Integration/Auth/CorrelationIdPropagationTests.cs` — Integration tests that prove the full correlation-ID chain over a live HTTP round-trip: `CorrelationIdMiddleware` (outermost) either reuses a caller-supplied `X-Correlation-Id` or mints a fresh one, then `GlobalExceptionMiddleware` echoes that same ID into the `ProblemDetails` response body; the test asserts the header ID and body ID always match, and that a supplied ID is never regenerated.

**Concepts reinforced today:**
- Cache-Aside (Lazy Loading) pattern — read from cache first; on a miss, read from the source of truth and populate the cache; invalidate on writes
- Interface Segregation / Dependency Inversion — the Application layer depending on `ICacheService` (not `IDistributedCache`) keeps infrastructure leakage out of business logic
- Cache key management — centralising key generation prevents producer/consumer drift and is a prerequisite for correct invalidation
- Test doubles (Spy pattern) — a `SpyCacheService` that records interactions lets unit tests assert cache behaviour without a real Redis instance
- Integration test coverage of middleware ordering — proving that two middlewares collaborate correctly requires an end-to-end HTTP test, not a unit test of either in isolation
- ASP.NET Core layered configuration — environment-specific `appsettings.{Env}.json` files override base settings without modifying the committed default configuration

---

## 2026-06-28 — No changes detected

No new files or folders were added today.

---

## 2026-06-27 — No changes detected

No new files or folders were added today.

---

## 2026-06-26 — Correlation ID middleware added for structured request tracing

**New additions:**
- `Backend/API/MiddleWare/CorrelationIdMiddleware.cs` — Middleware that assigns a unique correlation ID to every inbound HTTP request (reusing an `X-Correlation-Id` header if the caller supplies one), opens a structured logging scope so every log line emitted during the request automatically carries the same ID, and echoes the ID back on the response header. Includes a `CorrelationIdMiddlewareExtensions` helper for clean registration in `Program.cs`.

**Concepts reinforced today:**
- Correlation IDs — attaching a unique, propagated identifier to each request is the foundational observability pattern that makes distributed log tracing possible; without it, log lines from concurrent requests are indistinguishable
- Structured logging scopes — `ILogger.BeginScope` injects ambient key-value pairs (like `CorrelationId`) into every log statement within the scope without polluting every call site
- ASP.NET Core middleware pipeline — custom middleware wraps `_next(context)` to run logic before and after downstream handlers, with `context.Items` used for passing per-request data and `Response.OnStarting` for writing response headers safely
- Header propagation — accepting an inbound correlation header and echoing it on the response enables end-to-end traceability across service boundaries and client bug reports

---

## 2026-06-25 — MCP lazy-connect refactor and McpStartupService removed

**New additions:**
- No new files added. This was a refactoring commit that deleted `Backend/Infrastructure/MCP/McpStartupService.cs` and changed how the MCP client connects.

**What changed:**
- `Backend/Infrastructure/MCP/McpStartupService.cs` — **deleted**. Previously, this `IHostedService` eagerly connected the in-process MCP client at application startup using `IHostApplicationLifetime.ApplicationStarted`. It was removed in favour of a lazy-connect approach.
- `Backend/Infrastructure/MCP/McpService.cs` — refactored to establish the MCP client connection on first use (lazy initialisation) rather than at host startup. Removes a tight coupling between application boot time and MCP server readiness.
- `Backend/Application/Common/Interfaces/Services/IMcpService.cs` — updated to reflect the simplified interface contract after removing the startup-time wiring.
- `Backend/API/Program.cs` — removed the `McpStartupService` hosted-service registration.
- `Backend/Tests/Integration/IntegrationTestFactory.cs` — simplified now that no hosted service manages the MCP lifecycle separately.

**Concepts reinforced today:**
- Lazy initialisation — deferring expensive or order-sensitive operations (like connecting to a service) until first use, rather than eagerly at startup, improves resilience and startup time
- Hosted services vs on-demand initialisation — `IHostedService` is the right tool for background work, but connection setup that can fail or depend on request context is better handled lazily
- Refactoring for simplicity — removing an abstraction (`McpStartupService`) when its responsibility can be absorbed into an existing class (`McpService`) reduces moving parts and makes the system easier to reason about
- Dependency direction — keeping startup concerns in `Program.cs` and runtime concerns in the service class maintains clean separation

---

## 2026-06-25 — Integration test infrastructure and Claude sub-agents added

**New additions:**
- `.claude/agents/codebase-expert.md` — A Claude Code sub-agent definition specialised for navigating and reasoning about this codebase. Demonstrates the sub-agent pattern: instead of one monolithic AI session, you compose focused agents with scoped roles and context, letting each operate with tighter, more accurate knowledge.
- `.claude/agents/dev-planner.md` — A second sub-agent for planning development tasks — breaking features into steps, spotting risks, and producing structured implementation plans. Illustrates the single-responsibility principle applied to AI agents: separate the "what to build" reasoning from the "how the codebase works" reasoning.
- `Backend/Tests/Integration/IntegrationTestFactory.cs` — A shared `WebApplicationFactory<Program>` fixture that boots the real ASP.NET Core host inside tests, wiring Testcontainers for a throwaway MongoDB instance. This is the foundation for integration testing: the host, DI container, middleware pipeline, and real infrastructure all run together so tests exercise end-to-end HTTP behaviour rather than mocks.
- `Backend/Tests/Integration/Auth/SignupEndpointValidationTests.cs` — Integration tests that POST to the `/auth/signup` endpoint and assert that invalid payloads are correctly rejected (HTTP 400 + validation errors). Validates that FluentValidation, the pipeline behavior, and `GlobalExceptionMiddleware` all cooperate correctly over a real HTTP round-trip.
- `Backend/Tests/Integration/Auth/SignupPersistenceTests.cs` — Integration tests that POST a valid signup payload and assert the resulting user was actually written to MongoDB. Proves that the entire signup slice — controller → MediatR → handler → repository → database — works end-to-end, not just in isolation.

**Concepts reinforced today:**
- Integration testing vs unit testing — unit tests verify logic in isolation; integration tests verify that all the layers (routing, DI, middleware, persistence) collaborate correctly under realistic conditions
- WebApplicationFactory + Testcontainers — the standard .NET pattern for spinning up the real host and a real database in tests without a live environment
- Test pyramid — adding integration tests at the top of the pyramid, complementing the unit tests already covering the Domain and Application layers
- Sub-agent composition — decomposing AI assistance into focused, single-responsibility agents mirrors the same SRP and separation-of-concerns principles applied throughout the codebase

---

## 2026-06-24 — Claude tooling and project-level skill scaffold added

**New additions:**
- `.claude/settings.local.json` — Claude Code local settings file, scoped to this repository. Demonstrates how AI-assisted development workflows can be configured per-project, keeping tool preferences version-controlled alongside source code.
- `.claude/skills/scaffold-feature/SKILL.md` — A custom Claude Code skill definition for scaffolding new feature slices. Shows how repetitive CQRS boilerplate (Command → Handler → Validator → Controller action) can be encoded as a reusable prompt skill, automating the creation of correctly structured feature folders.
- `CLAUDE_CODE_GUIDE.md` — A guide documenting how Claude Code is used within this project — conventions, workflows, and tips specific to the codebase. Illustrates the practice of co-locating AI-workflow documentation with the code it supports, treating AI tooling as a first-class project dependency.

**Concepts reinforced today:**
- Developer tooling as code — storing IDE/AI tool configs in the repo ensures every contributor (human or AI) uses the same conventions
- Prompt engineering for code generation — encoding architectural patterns into skill definitions so scaffolding stays consistent with Clean Architecture rules
- Convention documentation — capturing *how* a project uses its tools is as important as the tools themselves

---

## 2026-06-24 — Signup validator added and test coverage extended to Application layer

**New additions:**
- `CLAUDE.md` — Project-level instructions file for the Claude AI assistant. Demonstrates how development teams document conventions and constraints directly in the repository so tooling (and humans) can follow consistent patterns automatically.
- `Backend/Application/Features/Auth/Commands/Signup/SignupCommandValidator.cs` — A FluentValidation validator for the `SignupCommand`. Completes the validation triad (Command → Validator → Handler) for the Signup feature, enforcing input rules (e.g. required fields, email format, password strength) at the Application layer boundary before the handler runs.
- `Backend/Tests/Application/Features/Auth/SignupCommandValidatorTests.cs` — Unit tests targeting the new `SignupCommandValidator`. Introduces the `Tests/Application/` subtree, extending the test project structure beyond domain-only coverage and following the same project-mirroring convention established by `Tests/Domain/`.

**Concepts reinforced today:**
- FluentValidation + MediatR pipeline — validators plugged in as pipeline behaviours keep handlers free of boilerplate guard clauses
- Input validation at the Application boundary — validating commands before they reach the domain prevents invalid state from ever entering business logic
- Test project mirroring — `Tests/Application/Features/Auth/` mirrors `Application/Features/Auth/`, keeping tests co-located and discoverable
- Expanding test coverage incrementally — growing the test suite one feature slice at a time rather than in bulk

---

## 2026-06-23 — First real unit test added for Email value object

**New additions:**
- `Backend/Tests/Domain/ValueObjects/EmailTests.cs` — The first concrete unit test file in the project, covering the `Email` value object. Replaces the previously scaffolded empty folder and the placeholder `UnitTest1.cs`, marking the transition from test infrastructure setup to actual test coverage.

**Concepts reinforced today:**
- Unit testing value objects — verifying that factory-method validation, equality-by-value, and normalisation rules behave correctly in isolation
- Test project mirroring — `Tests/Domain/ValueObjects/` directly mirrors the `Domain/ValueObjects/` layout, keeping tests discoverable alongside the code they cover
- Red-Green cycle discipline — scaffolding the folder (yesterday) then writing the test (today) follows the standard TDD setup-then-implement flow

---

## 2026-06-22 — Custom MongoDB serializer and test structure scaffolded

**New additions:**
- `Backend/Infrastructure/Persistence/Serializers/EmailSerializer.cs` — A custom MongoDB BSON serializer for the `Email` value object. Teaches how to bridge domain value objects to a document database: rather than persisting the raw struct, this serializer controls exactly how `Email` is written to and read from MongoDB, keeping the database representation clean (a plain string) while the in-memory type remains a rich value object.
- `Backend/Tests/Domain/ValueObjects/` — New test folder scaffolded under `Tests/Domain/ValueObjects/`. Establishes the directory structure for unit tests targeting domain value objects, mirroring the Domain project's layout inside the test project — a standard convention for keeping tests discoverable and co-located with the code they cover.
- `Frontend/Dashboard/src/app/Core/Validators/` — Empty validators folder added to the Angular Core module. Signals intent to centralise custom Angular `Validator` functions (e.g. for reactive forms) in a shared, reusable location rather than scattering them across feature modules.

**Concepts reinforced today:**
- Custom BSON/MongoDB serialization — adapting domain types to persistence without polluting the domain model
- Persistence Ignorance — the `Email` value object remains unaware of MongoDB; the serializer is purely an infrastructure concern
- Test project mirroring — structuring unit tests to match source project layout for discoverability
- Angular reactive forms validation pattern — centralising validators as a cross-cutting concern in the Core module

---

## 2026-06-21 — DDD Value Object pattern introduced

**New additions:**
- `Backend/Domain/Common/ValueObject.cs` — Abstract base class for Domain-Driven Design value objects. Overrides `Equals`, `GetHashCode`, and equality operators based on a list of components returned by `GetEqualityComponents()`, ensuring two instances are equal when all their fields are equal (unlike entities, which are equal by Id).
- `Backend/Domain/ValueObjects/Email.cs` — A concrete value object for email addresses implemented as a C# `record`. Uses a private constructor and a static `Create()` factory method to guarantee that an `Email` instance is always valid and normalised (trimmed, lowercased) — making invalid state unrepresentable.

**Concepts reinforced today:**
- Value Object pattern (DDD) — identity-by-value rather than identity-by-reference
- Making illegal state unrepresentable via private constructors + factory methods
- C# `record` types for compiler-generated structural equality
- Separation of domain validation from application-layer validation

---

## 2026-06-20 — Initial snapshot: full project catalogued

**New additions (first-run baseline):**

- `Backend/Domain/` — Domain entities (`User`, `BlogPost`, `BlogComment`, `BlogLike`, `Chapter`, `Subject`, `ChatThread`, `ToolCallRecord`) plus domain exceptions and repository interfaces. Demonstrates the Domain layer in Clean Architecture: pure business objects with no infrastructure dependencies.
- `Backend/Application/` — CQRS feature slices (Auth, Blog, Chapters, Chat, Courses, Lessons) each with Commands, Queries, DTOs, and Handlers. Shows the Command Query Responsibility Segregation (CQRS) pattern via MediatR, keeping reads and writes explicitly separated.
- `Backend/Application/Common/Interfaces/` — Abstraction interfaces for persistence, repositories, security, messaging, and services. Enforces the Dependency Inversion Principle: the Application layer depends on abstractions, not concrete infrastructure types.
- `Backend/Application/Common/Behaviors/ValidationBehavior.cs` — MediatR pipeline behaviour for request validation. Illustrates the Pipeline/Decorator pattern: cross-cutting concerns (validation) are injected into the request pipeline without touching handlers.
- `Backend/Infrastructure/` — Concrete implementations: MongoDB persistence, repository implementations, LLM clients (Claude, Gemini), MCP service, SignalR notification hub, email sender, and password hasher. Demonstrates the Adapter and Repository patterns bridging the domain to external systems.
- `Backend/Infrastructure/Llm/` (`ClaudeChatClient.cs`, `GeminiChatClient.cs`, `LlmFactory.cs`) — Multiple LLM provider implementations behind a factory. Shows the Factory and Strategy patterns for runtime provider selection.
- `Backend/Infrastructure/MCP/` — Model Context Protocol service and startup wiring. Demonstrates integration of an external agentic protocol into a .NET application.
- `Backend/Infrastructure/SignalR/` — Real-time notification hub and service. Introduces WebSocket-based push notifications via SignalR (Observer / Publish-Subscribe pattern).
- `Backend/API/Controllers/` — REST API entry points for Auth, Blog, Chapters, Chat, Courses, LessonDetails, and Notifications. Shows thin controllers that delegate all logic to MediatR commands/queries.
- `Backend/API/Extensions/` — Startup extension methods grouping DI registrations (`MasstransitAndMediatRExtensions`, `ServiceCollectionExtensions`). Demonstrates the Extension Method pattern for clean, modular service registration.
- `Backend/API/MiddleWare/GlobalExceptionMiddleware.cs` — Centralised exception handling. Illustrates the Middleware pattern for cross-cutting concerns in ASP.NET Core.
- `Frontend/Dashboard/src/app/Core/Store/` — NgRx store slices (actions, effects, reducers, selectors) for Subject state. Demonstrates the Redux pattern / Flux architecture in an Angular SPA.
- `Frontend/Dashboard/src/app/Core/Services/signalr.service.ts` — Angular SignalR client service. Shows frontend integration with real-time backend events.
- `Frontend/Dashboard/src/app/Features/` — Feature-sliced Angular modules: Auth (login, signup, profile, reset-password), Blog (CRUD UI), Courses (subjects, lessons, chapters), and Dashboard. Mirrors the backend vertical-slice structure on the frontend.
- `Frontend/Dashboard/src/app/Shared/` — Reusable components (header, footer, sidebar, chat, confirm-dialog) and shared models. Demonstrates component decomposition and the separation of shared vs feature-specific UI.
- `docker-compose.yml` — Multi-container orchestration for the full stack. Shows containerisation and service wiring with Docker Compose.

**Concepts reinforced today:**
- Clean Architecture (Domain → Application → Infrastructure → API layering)
- CQRS + MediatR (commands vs queries, pipeline behaviours)
- Repository Pattern and Unit of Work
- Dependency Inversion / Interface Segregation
- Factory and Strategy patterns (LLM provider selection)
- Adapter Pattern (infrastructure implementations)
- Middleware / Pipeline pattern (exception handling, validation)
- Observer / Pub-Sub (SignalR real-time notifications)
- Redux / Flux (NgRx store in Angular)
- Containerisation with Docker Compose

---
