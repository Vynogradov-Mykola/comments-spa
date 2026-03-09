namespace Comments.Api.Models
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Информация о пользователе
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? HomePage { get; set; }

        public Guid? ParentId { get; set; }
        public string? FileBase64 { get; set; }
        public string? FileName { get; set; }
        public string? FileContentType { get; set; }
    }
}