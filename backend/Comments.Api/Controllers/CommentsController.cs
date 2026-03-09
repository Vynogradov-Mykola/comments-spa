using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Comments.Api.Data;
using Comments.Api.Models;
using Comments.Api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.Text.Encodings.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
    
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
                FileName = c.FileName,
                FileContentType = c.FileContentType,
                FileBase64 = c.FileData != null ? Convert.ToBase64String(c.FileData) : null
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromForm] CommentCreateDto dto)
    {
        Console.WriteLine("===== CREATE COMMENT DEBUG =====");
        Console.WriteLine($"UserName: {dto.UserName}");
        Console.WriteLine($"Email: {dto.Email}");
        Console.WriteLine($"HomePage: {dto.HomePage}");
        Console.WriteLine($"CommentText: {dto.CommentText}");
        Console.WriteLine($"ParentId: {dto.ParentId}");
        Console.WriteLine($"CaptchaId: {dto.CaptchaId}");
        Console.WriteLine($"CaptchaText: {dto.CaptchaCode}");
        Console.WriteLine($"File: {(dto.File != null ? dto.File.FileName : "NULL")}");
        Console.WriteLine("=================================");
        if (!_captcha.Validate(dto.CaptchaId, dto.CaptchaCode))
            return BadRequest("Invalid CAPTCHA");

        if (string.IsNullOrWhiteSpace(dto.UserName) ||
    string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Required fields missing");
        }

        if (string.IsNullOrWhiteSpace(dto.CommentText) && dto.File == null)
        {
            return BadRequest("Comment or file required");
        }
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
    !new EmailAddressAttribute().IsValid(dto.Email))
        {
            return BadRequest("Invalid email");
        }

        var safeUserName = dto.UserName.Trim();
        
        var safeHomePage = dto.HomePage?.Trim();
        var safeCommentText = Regex.Replace(dto.CommentText, "<.*?>", "");
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == dto.Email && u.UserName == safeUserName);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = safeUserName,
                Email = dto.Email,
                HomePage = safeHomePage,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        byte[]? fileData = null;
        string? fileName = null;
        string? fileContentType = null;

        if (dto.File != null)
        {
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "text/plain"
            };

            if (!allowedTypes.Contains(dto.File.ContentType))
                return BadRequest("Unsupported file type");

            if (dto.File.ContentType == "text/plain" && dto.File.Length > 100 * 1024)
                return BadRequest("Text file too large");

            if (dto.File.Length > 2 * 1024 * 1024)
                return BadRequest("File too large");

            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            fileData = ms.ToArray();

            fileName = Path.GetFileName(dto.File.FileName);
            fileContentType = dto.File.ContentType;
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CommentText = safeCommentText,
            CreatedAt = DateTime.UtcNow,
            ParentId = dto.ParentId,
            FileData = fileData,
            FileName = fileName,
            FileContentType = fileContentType
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var result = new CommentDto
        {
            Id = comment.Id,
            CommentText = comment.CommentText,
            CreatedAt = comment.CreatedAt,
            UserName = user.UserName,
            Email = user.Email,
            HomePage = user.HomePage,
            ParentId = comment.ParentId,
            FileName = comment.FileName,
            FileContentType = comment.FileContentType,
            FileBase64 = comment.FileData != null
                ? Convert.ToBase64String(comment.FileData)
                : null
        };

        return Ok(result);
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
    public IFormFile? File { get; set; }
    public byte[]? FileData { get; set; }
    public string? FileName { get; set; }
    public string? FileContentType { get; set; }
}