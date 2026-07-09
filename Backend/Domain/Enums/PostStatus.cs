namespace Domain.Enums;

public enum PostStatus
{
    // Author created the post; it awaits admin review and is not publicly visible.
    Pending,
    // Admin approved it: visible in the public feed and emailed to subscribers.
    Published,
    // Admin declined it: hidden from the public feed.
    Rejected
}
