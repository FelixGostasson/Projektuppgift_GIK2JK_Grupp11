namespace BiluthyrningVS2022;
// Strategy separerar prisregeln från bokningsmotorn.
public interface IPrisstrategi { decimal Berakna(Fordon fordon, Hyrperiod period); }
public sealed class Dygnspris : IPrisstrategi
{
    public decimal Berakna(Fordon fordon, Hyrperiod period) => fordon.PrisPerDygn * period.AntalDygn;
}
public sealed class Veckorabatt : IPrisstrategi
{
    public decimal Berakna(Fordon fordon, Hyrperiod period) =>
        fordon.PrisPerDygn * period.AntalDygn * (period.AntalDygn >= 7 ? 0.9m : 1m);
}
// Decorator omsluter samma gränssnitt. Flera tillägg kan kombineras.
public sealed class Tillaggspris : IPrisstrategi
{
    private readonly IPrisstrategi _inre;
    private readonly Tillagg _tillagg;
    public Tillaggspris(IPrisstrategi inre, Tillagg tillagg)
    {
        _inre = inre ?? throw new ArgumentNullException(nameof(inre));
        _tillagg = tillagg ?? throw new ArgumentNullException(nameof(tillagg));
    }
    public decimal Berakna(Fordon fordon, Hyrperiod period) =>
        _inre.Berakna(fordon, period) + _tillagg.PrisPerDygn * period.AntalDygn;
}
