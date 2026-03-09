using Comments.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly CaptchaService _captchaService;

    public CaptchaController(CaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    [HttpGet]
    public IActionResult GetCaptcha()
    {
        var result = _captchaService.Generate();

        // получаем текст через метод GetCode
        var code = _captchaService.GetCode(result.Id);
        Console.WriteLine($"[Captcha Generated] Id: {result.Id}, Text: {code}");

        // безопасно добавляем или обновляем заголовок
        Response.Headers["X-Captcha-Id"] = result.Id;

        return File(result.Image, "image/png");
    }
}