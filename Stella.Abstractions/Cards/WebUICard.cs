namespace Stella.Abstractions.Cards;

/// <summary>Card row exposed to the WebUI.</summary>
public sealed class WebUICard
{
    public required string RefId { get; init; }
    public string? CardId { get; init; }
    public int Paseli { get; init; }
    public string? PassCode { get; init; }
}
