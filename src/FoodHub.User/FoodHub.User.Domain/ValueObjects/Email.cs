using System;
using System.Text.RegularExpressions;

namespace FoodHub.User.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.DomainException("Email cannot be empty.");

        var normalizedEmail = value.Trim().ToLowerInvariant();
        
        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new Exceptions.DomainException("Email format is invalid.");

        Value = normalizedEmail;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new Email(value);
}