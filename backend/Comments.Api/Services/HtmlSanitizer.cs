
using System.Text.RegularExpressions;

namespace Comments.Api.Services;

public static class HtmlSanitizer
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // удалить все теги кроме разрешенных
        input = Regex.Replace(input,
            @"<(?!\/?(a|code|i|strong)(\s|>|$))[^>]*>",
            "",
            RegexOptions.IgnoreCase);

        // очистить атрибуты кроме href и title у <a>
        input = Regex.Replace(input,
            @"<a\s+(?!href=|title=)[^>]+>",
            "<a>",
            RegexOptions.IgnoreCase);

        // оставить только href и title
        input = Regex.Replace(input,
            @"<a\s+([^>]*?)>",
            match =>
            {
                var tag = match.Value;

                var href = Regex.Match(tag, @"href\s*=\s*""[^""]*""");
                var title = Regex.Match(tag, @"title\s*=\s*""[^""]*""");

                return $"<a {href.Value} {title.Value}>".Trim();
            },
            RegexOptions.IgnoreCase);

        return input;
    }

    public static bool IsValidXhtml(string input)
    {
        var stack = new Stack<string>();

        var tags = Regex.Matches(input, @"</?([a-z]+)[^>]*>");

        foreach (Match tag in tags)
        {
            var name = tag.Groups[1].Value.ToLower();

            if (!tag.Value.StartsWith("</"))
            {
                stack.Push(name);
            }
            else
            {
                if (stack.Count == 0 || stack.Pop() != name)
                    return false;
            }
        }

        return stack.Count == 0;
    }
}