using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Condubello.Infrastructure.Persistence.Converters;

/// <summary>Guarda los enums como texto en MAYÚSCULAS_CON_GUIONES (ej. EnCurso → EN_CURSO).</summary>
public partial class UpperSnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    v => ToUpperSnake(v.ToString()),
    v => Enum.Parse<TEnum>(v.Replace("_", string.Empty), true))
    where TEnum : struct, Enum
{
    private static string ToUpperSnake(string value) =>
        PascalBoundary().Replace(value, "$1_$2").ToUpperInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex PascalBoundary();
}
