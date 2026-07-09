using FluentValidation;

namespace Application.Features.Posts.Commands.CreatePost;

// Declarative validation rules for CreatePostCommand. The ValidationBehavior
// discovers and runs this automatically before CreatePostCommandHandler.
public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
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
