namespace Comments.Api.Models;

public class Comment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public Guid? ParentId { get; set; }

    public Comment? Parent { get; set; }

    public string CommentText { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ImagePath { get; set; }

    public string? FilePath { get; set; }
}