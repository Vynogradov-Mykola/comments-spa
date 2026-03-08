namespace Comments.Api.Models;

public class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string? HomePage { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; }
}