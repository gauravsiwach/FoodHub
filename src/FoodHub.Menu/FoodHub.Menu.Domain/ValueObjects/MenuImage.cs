using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.ValueObjects;

public sealed record MenuImage
{
    public string Type { get; }
    public string Url { get; }

    public MenuImage(string type, string url)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new DomainException("Image type cannot be empty.");
        if (string.IsNullOrWhiteSpace(url)) throw new DomainException("Image url cannot be empty.");

        Type = type;
        Url = url;
    }
}
