namespace Application.Settings;

public sealed class ShippingSettings
{
    public const string SectionName = "Shipping";

    public decimal Fee { get; init; }
}
