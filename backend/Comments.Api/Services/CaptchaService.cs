using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Comments.Api.Services;

public class CaptchaService
{
    private readonly Dictionary<string, string> _store = new();
    private readonly Random _random = new();

    public (string Id, byte[] Image) Generate()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var code = new string(
            Enumerable.Range(0, 5)
                .Select(_ => chars[_random.Next(chars.Length)])
                .ToArray());

        var id = Guid.NewGuid().ToString();

        _store[id] = code;

        int width = 180;
        int height = 70;

        using var image = new Image<Rgba32>(width, height);

        image.Mutate(ctx =>
        {
            ctx.Fill(Color.White);

            var font = SystemFonts.CreateFont("DejaVu Sans", 36, FontStyle.Bold);

            ctx.DrawText(
                code,
                font,
                Color.Black,
                new PointF(20, 15)
            );

            // линии
            for (int i = 0; i < 8; i++)
            {
                ctx.DrawLine(
                    Color.Gray,
                    1,
                    new PointF(_random.Next(width), _random.Next(height)),
                    new PointF(_random.Next(width), _random.Next(height)));
            }

            // шум
            for (int i = 0; i < 100; i++)
            {
                ctx.Fill(
                    Color.LightGray,
                    new Rectangle(_random.Next(width), _random.Next(height), 2, 2));
            }
        });

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());

        return (id, ms.ToArray());
    }

    public bool Validate(string id, string code)
    {
        if (!_store.ContainsKey(id))
            return false;

        var valid = _store[id]
            .Equals(code, StringComparison.OrdinalIgnoreCase);

        _store.Remove(id); // удаляем после проверки

        return valid;
    }

    // Новый метод для логирования ожидаемого кода
    public string? GetCode(string id)
    {
        if (_store.TryGetValue(id, out var code))
            return code;
        return null;
    }
}