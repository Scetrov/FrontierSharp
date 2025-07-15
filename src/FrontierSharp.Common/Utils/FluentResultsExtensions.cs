using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace FrontierSharp.Common.Utils;

public static class FluentResultsExtensions {
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public static string ToErrorString<T>(this IResult<T> result) {
        if (result.IsSuccess) return string.Empty;

        return string.Join(Environment.NewLine, result.Errors.Select(e => $"- {e.Message}"));
    }
}