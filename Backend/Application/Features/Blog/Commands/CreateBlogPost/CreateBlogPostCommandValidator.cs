using FluentValidation;

namespace Application.Features.Blog.Commands.CreateBlogPost;

// Declarative validation rules for CreateBlogPostCommand. The ValidationBehavior
// discovers and runs this automatically before CreateBlogPostCommandHandler.
public class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostCommandValidator()
    {
        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary must be 500 characters or fewer.")
            .When(x => !string.IsNullOrWhiteSpace(x.Summary));
    }
}
