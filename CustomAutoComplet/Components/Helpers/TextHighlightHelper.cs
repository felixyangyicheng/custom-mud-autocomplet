using Microsoft.AspNetCore.Components;

namespace CustomAutoComplet.Components.Helpers;

public static class TextHighlightHelper
{
    public static RenderFragment Highlight(
        string text,
        string keyword)
    {
        if (string.IsNullOrWhiteSpace(text) ||
            string.IsNullOrWhiteSpace(keyword))
            return builder => builder.AddContent(0, text);

        var index = text.IndexOf(
            keyword,
            StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return builder => builder.AddContent(0, text);

        return builder =>
        {
            builder.AddContent(0, text[..index]);

            builder.OpenElement(1, "mark");
            builder.AddAttribute(2, "class", "highlight");
            builder.AddContent(3, text.Substring(index, keyword.Length));
            builder.CloseElement();

            builder.AddContent(4, text[(index + keyword.Length)..]);
        };
    }
}