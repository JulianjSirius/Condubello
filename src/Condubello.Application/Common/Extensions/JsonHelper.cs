using System.Text.Json;

namespace Condubello.Application.Common.Extensions;

public static class JsonHelper
{
    public static string? ToJson(JsonElement? element) =>
        element is { ValueKind: not (JsonValueKind.Undefined or JsonValueKind.Null) } value
            ? value.GetRawText()
            : null;

    public static JsonElement? Parse(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<JsonElement>(json);
}
