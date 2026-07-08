# Project Structure

> Auto-generated on 2026-07-07

```
Software-engineering-learning-Blueprint-fork/
├── .claude/
│   ├── agent-memory/
│   │   └── codebase-expert/
│   ├── agents/
│   │   ├── codebase-expert.md
│   │   └── dev-planner.md
│   ├── settings.local.json
│   └── skills/
├── .github/
│   └── workflows/
├── .gitignore
├── CLAUDE.md
├── docker-compose.yml
├── DAILY_LEARNING_LOG.md
├── LEARNING_LOG.md
├── PROJECT_STRUCTURE.md
├── PROJECT_TECH_STACK.md
├── README.md
│
├── Backend/
│   ├── BackendBluePrint.slnx
│   ├── backend-documentation.md
│   ├── Dockerfile
│   │
│   ├── API/
│   │   ├── API.csproj
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Dockerfile
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── BlogController.cs
│   │   │   ├── ChaptersController.cs
│   │   │   ├── ChatController.cs
│   │   │   ├── CourseController.cs
│   │   │   ├── HomeController.cs
│   │   │   ├── LessonDetailsController.cs
│   │   │   └── NotificationController.cs
│   │   ├── Extensions/
│   │   │   ├── ConfigurationSettingExtensions.cs
│   │   │   ├── MasstransitAndMediatRExtensions.cs
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── MiddleWare/
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   └── GlobalExceptionMiddleware.cs
│   │   └── Properties/
│   │       └── launchSettings.json
│   │
│   ├── Application/
│   │   ├── Application.csproj
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   ├── Events/
│   │   │   │   ├── DomainEventNotification.cs
│   │   │   │   └── IDomainEventDispatcher.cs
│   │   │   ├── Helper/
│   │   │   │   └── TellMe.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── Persistence/
│   │   │   │   │   └── IDatabaseContext.cs
│   │   │   │   ├── Publisher/
│   │   │   │   │   └── IMessageBus.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── IBlogCommentRepository.cs
│   │   │   │   │   ├── IBlogLikeRepository.cs
│   │   │   │   │   ├── IBlogPostRepository.cs
│   │   │   │   │   ├── ICourseRepository.cs
│   │   │   │   │   └── IUserRepository.cs
│   │   │   │   ├── Security/
│   │   │   │   │   ├── IAuthValidator.cs
│   │   │   │   │   └── IPasswordHasher.cs
│   │   │   │   └── Services/
│   │   │   │       ├── ICacheService.cs
│   │   │   │       ├── IChatHistoryStore.cs
│   │   │   │       ├── IEmailSender.cs
│   │   │   │       ├── ILlmFactory.cs
│   │   │   │       ├── IMcpService.cs
│   │   │   │       └── INotificationService.cs
│   │   │   └── Security/
│   │   │       ├── AuthValidator.cs
│   │   │       └── ResetTokenUtil.cs
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── ForgotPassword/
│   │   │   │   │   │   ├── ForgotPasswordCommand.cs
│   │   │   │   │   │   └── ForgotPasswordCommandHandler.cs
│   │   │   │   │   ├── ResetPassword/
│   │   │   │   │   │   ├── ResetPasswordCommand.cs
│   │   │   │   │   │   └── ResetPasswordCommandHandler.cs
│   │   │   │   │   ├── Signup/
│   │   │   │   │   │   ├── SignupCommand.cs
│   │   │   │   │   │   ├── SignupCommandHandler.cs
│   │   │   │   │   │   └── SignupCommandValidator.cs
│   │   │   │   │   └── UpdateProfile/
│   │   │   │   │       ├── UpdateProfileCommand.cs
│   │   │   │   │       └── UpdateProfileCommandHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── AuthResponseDto.cs
│   │   │   │   │   ├── ForgotPasswordRequestDto.cs
│   │   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   │   ├── MessageResponseDto.cs
│   │   │   │   │   ├── ResetPasswordRequestDto.cs
│   │   │   │   │   ├── SignupRequestDto.cs
│   │   │   │   │   └── UpdateProfileRequestDto.cs
│   │   │   │   └── Queries/
│   │   │   │       ├── GetUserById/
│   │   │   │       │   ├── GetUserByIdQuery.cs
│   │   │   │       │   └── GetUserByIdQueryHandler.cs
│   │   │   │       └── Login/
│   │   │   │           ├── LoginQuery.cs
│   │   │   │           └── LoginQueryHandler.cs
│   │   │   │   ├── Events/
│   │   │   │   │   └── DomainEventHandler.cs
│   │   │   ├── Blog/
│   │   │   │   ├── BlogCacheKeys.cs
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── AddComment/
│   │   │   │   │   │   ├── AddCommentCommand.cs
│   │   │   │   │   │   └── AddCommentCommandHandler.cs
│   │   │   │   │   ├── CreateBlogPost/
│   │   │   │   │   │   ├── CreateBlogPostCommand.cs
│   │   │   │   │   │   ├── CreateBlogPostCommandHandler.cs
│   │   │   │   │   │   └── CreateBlogPostCommandValidator.cs
│   │   │   │   │   ├── DeleteBlogPost/
│   │   │   │   │   │   ├── DeleteBlogPostCommand.cs
│   │   │   │   │   │   └── DeleteBlogPostCommandHandler.cs
│   │   │   │   │   ├── DeleteComment/
│   │   │   │   │   │   ├── DeleteCommentCommand.cs
│   │   │   │   │   │   └── DeleteCommentCommandHandler.cs
│   │   │   │   │   ├── ToggleLike/
│   │   │   │   │   │   ├── ToggleLikeCommand.cs
│   │   │   │   │   │   └── ToggleLikeCommandHandler.cs
│   │   │   │   │   └── UpdateBlogPost/
│   │   │   │   │       ├── UpdateBlogPostCommand.cs
│   │   │   │   │       └── UpdateBlogPostCommandHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── AddCommentRequestDto.cs
│   │   │   │   │   ├── BlogPostDetailDto.cs
│   │   │   │   │   ├── BlogPostSummaryDto.cs
│   │   │   │   │   ├── CommentDto.cs
│   │   │   │   │   ├── CreateBlogPostRequestDto.cs
│   │   │   │   │   ├── ToggleLikeResponseDto.cs
│   │   │   │   │   └── UpdateBlogPostRequestDto.cs
│   │   │   │   └── Queries/
│   │   │   │       ├── GetBlogPostById/
│   │   │   │       │   ├── GetBlogPostByIdQuery.cs
│   │   │   │       │   └── GetBlogPostByIdQueryHandler.cs
│   │   │   │       └── GetBlogPosts/
│   │   │   │           ├── GetBlogPostsQuery.cs
│   │   │   │           └── GetBlogPostsQueryHandler.cs
│   │   │   ├── Chapters/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateChapter/
│   │   │   │   │   │   ├── CreateChapterCommand.cs
│   │   │   │   │   │   └── CreateChapterCommandHandler.cs
│   │   │   │   │   ├── CreateLessonDetails/
│   │   │   │   │   │   ├── CreateLessonDetailsCommand.cs
│   │   │   │   │   │   └── CreateLessonDetailsCommandHandler.cs
│   │   │   │   │   ├── DeleteChapter/
│   │   │   │   │   │   ├── DeleteChapterCommand.cs
│   │   │   │   │   │   └── DeleteChapterCommandHandler.cs
│   │   │   │   │   ├── DeleteLessonDetails/
│   │   │   │   │   │   ├── DeleteLessonDetailsCommand.cs
│   │   │   │   │   │   └── DeleteLessonDetailsCommandHandler.cs
│   │   │   │   │   ├── UpdateChapter/
│   │   │   │   │   │   ├── UpdateChapterCommand.cs
│   │   │   │   │   │   └── UpdateChapterCommandHandler.cs
│   │   │   │   │   └── UpdateLessonDetails/
│   │   │   │   │       ├── UpdateLessonDetailsCommand.cs
│   │   │   │   │       └── UpdateLessonDetailsCommandHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChapterResponseDto.cs
│   │   │   │   │   └── LessonDetailsDto.cs
│   │   │   │   └── Query/
│   │   │   │       └── GetChaptersBySubjectId/
│   │   │   │           ├── GetChaptersBySubjectIdQuery.cs
│   │   │   │           └── GetChaptersBySubjectIdQueryHandler.cs
│   │   │   ├── Chat/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── SendChatCommand.cs
│   │   │   │   │   ├── SendChatCommandHandler.cs
│   │   │   │   │   └── SuggestAndSaveThreadTitle/
│   │   │   │   │       ├── SuggestAndSaveThreadTitleCommand.cs
│   │   │   │   │       └── SuggestAndSaveThreadTitleCommandHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChatRequestDto.cs
│   │   │   │   │   └── ChatResponseDto.cs
│   │   │   │   └── Queries/
│   │   │   │       └── SuggestThreadTitle/
│   │   │   │           ├── SuggestThreadTitleQuery.cs          (tombstone — promoted to Command, Day 26)
│   │   │   │           └── SuggestThreadTitleQueryHandler.cs   (tombstone — promoted to Command, Day 26)
│   │   │   ├── Courses/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateCourse/
│   │   │   │   │   │   ├── CreateCourseCommand.cs
│   │   │   │   │   │   └── CreateCourseCommandHandler.cs
│   │   │   │   │   ├── DeleteCourse/
│   │   │   │   │   │   ├── DeleteCourseCommand.cs
│   │   │   │   │   │   └── DeleteCourseCommandHandler.cs
│   │   │   │   │   └── UpdateCourse/
│   │   │   │   │       ├── UpdateCourseCommand.cs
│   │   │   │   │       └── UpdateCourseCommandHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChapterDto.cs
│   │   │   │   │   ├── ChapterResponseDto.cs
│   │   │   │   │   ├── CourseResponseDto.cs
│   │   │   │   │   ├── CreateCourseRequestDto.cs
│   │   │   │   │   └── UpdateSubjectDto.cs
│   │   │   │   └── Query/
│   │   │   │       ├── GetAllCourses/
│   │   │   │       │   ├── GetAllCoursesQuery.cs
│   │   │   │       │   └── GetAllCoursesQueryHandler.cs
│   │   │   │       └── GetCourseById/
│   │   │   │           ├── GetCourseByIdQuery.cs
│   │   │   │           └── GetCourseByIdQueryHandler.cs
│   │   │   └── Lessons/
│   │   │       ├── Command/
│   │   │       │   └── CreateLesson/
│   │   │       │       ├── CreateLessonCommand.cs
│   │   │       │       └── CreateLessonCommandHandler.cs
│   │   │       └── Query/
│   │   │           └── GetLessonDetailsByLessonId/
│   │   │               ├── GetLessonDetailsByLessonIdQuery.cs
│   │   │               └── GetLessonDetailsByLessonIdQueryHandler.cs
│   │   ├── Models/
│   │   │   └── Notifications/
│   │   │       └── NotificationDto.cs
│   │   ├── Settings/
│   │   │   ├── MongoSettings.cs
│   │   │   └── PasswordResetOptions.cs
│   │   └── Tools/
│   │       ├── DTOs/
│   │       │   └── ToolSummary.cs
│   │       ├── Queries/
│   │       │   ├── GetAvailableToolsQuery.cs
│   │       │   └── GetAvailableToolsQueryHandler.cs
│   │       └── TutorialTools.cs
│   │
│   ├── Contracts/
│   │   └── Contracts.csproj
│   │
│   ├── Domain/
│   │   ├── Domain.csproj
│   │   ├── Common/
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   └── ValueObject.cs
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── BlogComment.cs
│   │   │   ├── BlogLike.cs
│   │   │   ├── BlogPost.cs
│   │   │   ├── Chapter.cs
│   │   │   ├── ChatThread.cs
│   │   │   ├── Subject.cs
│   │   │   ├── ToolCallRecord.cs
│   │   │   └── User.cs
│   │   ├── Enums/
│   │   │   ├── LlmProvider.cs
│   │   │   └── NotificationType.cs
│   │   ├── Events/
│   │   │   └── UserRegisteredEvent.cs
│   │   ├── Exceptions/
│   │   │   ├── AuthenticationException.cs
│   │   │   ├── LlmUnavailableException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   ├── UnknownException.cs
│   │   │   └── ValidationException.cs
│   │   ├── Interfaces/
│   │   │   └── IUnitOfWork.cs
│   │   ├── Repositories/
│   │   │   └── Base/
│   │   │       └── IRepository.cs
│   │   └── ValueObjects/
│   │       └── Email.cs
│   │
│   ├── Infrastructure/
│   │   ├── Infrastructure.csproj
│   │   ├── Chat/
│   │   │   ├── InMemoryChatHistoryStore.cs
│   │   │   └── MongoChatHistoryStore.cs
│   │   ├── Configuration/
│   │   │   ├── BrevoEmailOptions.cs
│   │   │   ├── ClaudeOptions.cs
│   │   │   ├── GeminiOptions.cs
│   │   │   └── McpServerOptions.cs
│   │   ├── Helper/
│   │   │   └── ConfigurationHelper.cs
│   │   ├── Jobs/
│   │   │   └── HeartbitTestJob.cs
│   │   ├── Llm/
│   │   │   ├── ClaudeChatClient.cs
│   │   │   ├── GeminiChatClient.cs
│   │   │   ├── LlmFactory.cs
│   │   │   └── ResilientChatClient.cs
│   │   ├── MCP/
│   │   │   └── McpService.cs
│   │   ├── Persistence/
│   │   │   ├── DatabaseContext.cs
│   │   │   ├── MongoIndexInitializer.cs
│   │   │   ├── Indexing/
│   │   │   │   ├── BlogCommentIndexes.cs
│   │   │   │   ├── BlogLikeIndexes.cs
│   │   │   │   ├── BlogPostIndexes.cs
│   │   │   │   ├── IMongoIndexConfiguration.cs
│   │   │   │   └── MongoIndexConfiguration.cs
│   │   │   └── Serializers/
│   │   │       └── EmailSerializer.cs
│   │   ├── Repositories/
│   │   │   ├── Base/
│   │   │   │   └── Repository.cs
│   │   │   ├── BlogCommentRepository.cs
│   │   │   ├── BlogLikeRepository.cs
│   │   │   ├── BlogPostRepository.cs
│   │   │   ├── CourseRepository.cs
│   │   │   └── UserRepository.cs
│   │   ├── Security/
│   │   │   └── Pbkdf2PasswordHasher.cs
│   │   ├── Services/
│   │   │   ├── BrevoEmailSender.cs
│   │   │   ├── DomainEventDispatcher.cs
│   │   │   ├── MessageBus.cs
│   │   │   └── RedisCacheService.cs
│   │   └── SignalR/
│   │       ├── Hubs/
│   │       │   └── NotificationHub.cs
│   │       └── Services/
│   │           └── SignalRNotificationService.cs
│   │
│   └── Tests/
│       ├── Tests.csproj
│       ├── Application/
│       │   └── Features/
│       │       ├── Auth/
│       │       │   ├── SignupCommandValidatorTests.cs
│       │       │   └── SignupDomainEventPublishTests.cs
│       │       └── Blog/
│       │           └── BlogCacheAsideTests.cs
│       ├── Domain/
│       │   └── ValueObjects/
│       │       └── EmailTests.cs
│       └── Integration/
│           ├── IntegrationTestFactory.cs
│           └── Auth/
│               ├── CorrelationIdPropagationTests.cs
│               ├── SignupEndpointPersistenceTests.cs
│               ├── SignupEndpointValidationTests.cs
│               └── SignupPersistenceTests.cs
│
├── Playground/
│   └── LoggerFactoryDemo/                      (empty — scaffold for logging experiments)
│
└── Frontend/
    └── Dashboard/                          (Angular app)
        ├── .editorconfig
        ├── .gitignore
        ├── angular.json
        ├── Dockerfile
        ├── nginx.conf
        ├── package.json
        ├── package-lock.json
        ├── README.md
        ├── tsconfig.json
        ├── tsconfig.app.json
        ├── tsconfig.spec.json
        ├── public/
        │   └── favicon.ico
        └── src/
            ├── index.html
            ├── main.ts
            ├── styles.scss
            ├── app/
            │   ├── app.component.html
            │   ├── app.component.scss
            │   ├── app.component.ts
            │   ├── app.config.ts
            │   ├── app.routes.ts
            │   ├── Core/
            │   │   ├── Interceptors/
            │   │   │   └── user-id.interceptor.ts
            │   │   ├── Models/
            │   │   │   └── problem-details.ts
            │   │   ├── Services/
            │   │   │   ├── chat.service.ts
            │   │   │   ├── config.service.ts
            │   │   │   └── signalr.service.ts
            │   │   └── Store/
            │   │       ├── actions/
            │   │       │   └── subject.actions.ts
            │   │       ├── effects/
            │   │       │   └── subject.effects.ts
            │   │       ├── reducers/
            │   │       │   └── subject.reducer.ts
            │   │       └── selectors/
            │   │           └── subject.selectors.ts
            │   ├── Features/
            │   │   ├── Auth/
            │   │   │   ├── Components/
            │   │   │   │   ├── auth-modal/
            │   │   │   │   │   ├── auth-modal.component.html
            │   │   │   │   │   ├── auth-modal.component.scss
            │   │   │   │   │   └── auth-modal.component.ts
            │   │   │   │   └── profile/
            │   │   │   │       ├── profile.component.html
            │   │   │   │       ├── profile.component.scss
            │   │   │   │       └── profile.component.ts
            │   │   │   ├── Models/
            │   │   │   │   └── auth.model.ts
            │   │   │   ├── Pages/
            │   │   │   │   └── reset-password/
            │   │   │   │       ├── reset-password.component.html
            │   │   │   │       ├── reset-password.component.scss
            │   │   │   │       └── reset-password.component.ts
            │   │   │   └── Services/
            │   │   │       ├── auth-modal.service.ts
            │   │   │       └── auth.service.ts
            │   │   ├── Blog/
            │   │   │   ├── Components/
            │   │   │   │   ├── blog-card/
            │   │   │   │   │   ├── blog-card.html
            │   │   │   │   │   ├── blog-card.scss
            │   │   │   │   │   └── blog-card.ts
            │   │   │   │   ├── blog-detail/
            │   │   │   │   │   ├── blog-detail.html
            │   │   │   │   │   ├── blog-detail.scss
            │   │   │   │   │   └── blog-detail.ts
            │   │   │   │   ├── blog-form/
            │   │   │   │   │   ├── blog-form.html
            │   │   │   │   │   ├── blog-form.scss
            │   │   │   │   │   └── blog-form.ts
            │   │   │   │   └── blog-list/
            │   │   │   │       ├── blog-list.html
            │   │   │   │       ├── blog-list.scss
            │   │   │   │       └── blog-list.ts
            │   │   │   ├── Models/
            │   │   │   │   └── blog.model.ts
            │   │   │   └── Services/
            │   │   │       └── blog.service.ts
            │   │   ├── Courses/
            │   │   │   ├── Components/
            │   │   │   │   ├── home-lesson-component/
            │   │   │   │   │   ├── home-lesson-component.html
            │   │   │   │   │   ├── home-lesson-component.scss
            │   │   │   │   │   ├── home-lesson-component.spec.ts
            │   │   │   │   │   └── home-lesson-component.ts
            │   │   │   │   ├── lesson-component/
            │   │   │   │   │   ├── lesson-component.html
            │   │   │   │   │   ├── lesson-component.scss
            │   │   │   │   │   ├── lesson-component.spec.ts
            │   │   │   │   │   └── lesson-component.ts
            │   │   │   │   ├── lesson-create-component/
            │   │   │   │   │   ├── lesson-create-component.html
            │   │   │   │   │   ├── lesson-create-component.scss
            │   │   │   │   │   └── lesson-create-component.ts
            │   │   │   │   └── subjects-component/
            │   │   │   │       ├── subjects-component.html
            │   │   │   │       ├── subjects-component.scss
            │   │   │   │       ├── subjects-component.spec.ts
            │   │   │   │       └── subjects-component.ts
            │   │   │   ├── Models/
            │   │   │   │   ├── chapter.model.ts
            │   │   │   │   ├── lesson-details.model.ts
            │   │   │   │   └── subject.model.ts
            │   │   │   ├── Services/
            │   │   │   │   ├── chapter.service.ts
            │   │   │   │   ├── lesson-details.service.ts
            │   │   │   │   └── subject.service.ts
            │   │   │   └── courses.routes.ts
            │   │   └── dashboard/
            │   │       ├── Services/
            │   │       │   └── dashboard.service.ts
            │   │       ├── components/
            │   │       │   └── dashboard-home/
            │   │       │       ├── dashboard-home.html
            │   │       │       ├── dashboard-home.scss
            │   │       │       ├── dashboard-home.spec.ts
            │   │       │       └── dashboard-home.ts
            │   │       └── dashboard.routes.ts
            │   ├── Layouts/
            │   │   ├── course-layout-component/
            │   │   │   ├── course-layout-component.html
            │   │   │   ├── course-layout-component.scss
            │   │   │   ├── course-layout-component.spec.ts
            │   │   │   └── course-layout-component.ts
            │   │   └── main-layout-component/
            │   │       ├── main-layout-component.html
            │   │       ├── main-layout-component.scss
            │   │       ├── main-layout-component.spec.ts
            │   │       └── main-layout-component.ts
            │   └── Shared/
            │       ├── Components/
            │       │   ├── chat/
            │       │   │   ├── chat.component.html
            │       │   │   ├── chat.component.scss
            │       │   │   └── chat.component.ts
            │       │   ├── confirm-dialog/
            │       │   │   ├── confirm-dialog.component.html
            │       │   │   ├── confirm-dialog.component.scss
            │       │   │   └── confirm-dialog.component.ts
            │       │   ├── footer-component/
            │       │   │   ├── footer-component.html
            │       │   │   ├── footer-component.scss
            │       │   │   ├── footer-component.spec.ts
            │       │   │   └── footer-component.ts
            │       │   ├── header-component/
            │       │   │   ├── header-component.html
            │       │   │   ├── header-component.scss
            │       │   │   ├── header-component.spec.ts
            │       │   │   └── header-component.ts
            │       │   └── sidebar-component/
            │       │       ├── sidebar-component.html
            │       │       ├── sidebar-component.scss
            │       │       ├── sidebar-component.spec.ts
            │       │       └── sidebar-component.ts
            │       └── Models/
            │           └── notification.model.ts
            └── environments/
                ├── environment.development.ts
                └── environment.production.ts
```
