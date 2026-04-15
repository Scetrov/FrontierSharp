using System.Diagnostics;

namespace FrontierSharp.SuiClient.Models;

/// <summary>
///     Represents an opaque GraphQL pagination cursor.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class Cursor : IEquatable<Cursor> {
    public Cursor(string value) {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Cursor value cannot be null or whitespace.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(Cursor? other) {
        return other != null && StringComparer.Ordinal.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj) {
        return obj is Cursor other && Equals(other);
    }

    public override int GetHashCode() {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString() {
        return Value;
    }

    public static bool operator ==(Cursor? left, Cursor? right) {
        return Equals(left, right);
    }

    public static bool operator !=(Cursor? left, Cursor? right) {
        return !Equals(left, right);
    }
}