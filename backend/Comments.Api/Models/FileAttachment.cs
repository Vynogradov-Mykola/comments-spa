namespace Comments.Api.Models;

public class FileAttachment
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
}