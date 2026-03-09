using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Comments.Api.Data;
using Comments.Api.Models;
using Comments.Api.Services;
namespace Comments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CaptchaService _captcha;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(AppDbContext db, CaptchaService captcha, ILogger<CommentsController> logger)
    {
        _db = db;
        _captcha = captcha;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(
        string? sortBy = "CreatedAt",
        string sortOrder = "desc",
        int skip = 0,
        int take = 100)
    {
        var query = _db.Comments.Include(c => c.User).AsQueryable();

        query = (sortBy?.ToLower(), sortOrder.ToLower()) switch
        {
            ("username", "asc") => query.OrderBy(c => c.User.UserName),
            ("username", "desc") => query.OrderByDescending(c => c.User.UserName),
            ("email", "asc") => query.OrderBy(c => c.User.Email),
            ("email", "desc") => query.OrderByDescending(c => c.User.Email),
            ("createdat", "asc") => query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var comments = await query
            .Skip(skip)
            .Take(take)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                CommentText = c.CommentText,
                CreatedAt = c.CreatedAt,
                UserName = c.User.UserName,
                Email = c.User.Email,
                HomePage = c.User.HomePage,
                ParentId = c.ParentId,
                FilePath = c.FilePath,
                ImagePath = c.ImagePath
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto dto)
    {
        // Логирование капчи
        var expected = _captcha.GetCode(dto.CaptchaId);
        _logger.LogInformation("[Captcha Validation] Front sent: {Front}, Server expected: {Expected}",
            dto.CaptchaCode, expected);

        if (!_captcha.Validate(dto.CaptchaId, dto.CaptchaCode))
        {
            return BadRequest("Invalid CAPTCHA");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.UserName == dto.UserName);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName,
                Email = dto.Email,
                HomePage = dto.HomePage,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CommentText = dto.CommentText,
            ParentId = dto.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return Ok(comment);
    }
}

public class CommentCreateDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }
    public string CommentText { get; set; } = null!;
    public Guid? ParentId { get; set; }

    public string CaptchaId { get; set; } = null!;
    public string CaptchaCode { get; set; } = null!;
}