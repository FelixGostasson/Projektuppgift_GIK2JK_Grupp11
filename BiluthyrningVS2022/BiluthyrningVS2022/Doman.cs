namespace BiluthyrningVS2022;

public sealed class Hyrperiod
{
    public DateOnly Start { get; }
    public DateOnly Slut { get; }
    public int AntalDygn => Slut.DayNumber - Start.DayNumber;
    public Hyrperiod(DateOnly start, DateOnly slut)
    {
        if (slut <= start) throw new ArgumentException("Slutdatum måste vara efter startdatum.");
        Start = start; Slut = slut;
    }
    public bool Overlappar(Hyrperiod annan)
    {
        ArgumentNullException.ThrowIfNull(annan);
        return Start < annan.Slut && annan.Start < Slut;
    }
}
public abstract class Fordon
{
    public string Registreringsnummer { get; }
    public string Modell { get; }
    public abstract string Typ { get; }
    public decimal PrisPerDygn { get; }
    protected Fordon(string reg, string modell, decimal pris)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reg);
        ArgumentException.ThrowIfNullOrWhiteSpace(modell);
        if (pris <= 0) throw new ArgumentOutOfRangeException(nameof(pris));
        Registreringsnummer = reg.Trim().ToUpperInvariant(); Modell = modell.Trim(); PrisPerDygn = pris;
    }
}
public sealed class Kompaktbil(string reg, string modell, decimal pris) : Fordon(reg, modell, pris)
{ public override string Typ => "Kompakt"; }
public sealed class Kombi(string reg, string modell, decimal pris) : Fordon(reg, modell, pris)
{ public override string Typ => "Kombi"; }
public sealed class Elbil(string reg, string modell, decimal pris) : Fordon(reg, modell, pris)
{ public override string Typ => "Elbil"; }
public sealed class Skapbil(string reg, string modell, decimal pris) : Fordon(reg, modell, pris)
{ public override string Typ => "Skåpbil"; }
public sealed class Kund
{
    public string Id { get; }
    public string Namn { get; }
    public Kund(string id, string namn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(namn);
        Id = id.Trim(); Namn = namn.Trim();
    }
}
public sealed class Tillagg
{
    public string Namn { get; }
    public decimal PrisPerDygn { get; }
    public Tillagg(string namn, decimal pris)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namn);
        if (pris < 0) throw new ArgumentOutOfRangeException(nameof(pris));
        Namn = namn.Trim(); PrisPerDygn = pris;
    }
}
public enum Bokningsstatus { Bokad, Uthamtad, Aterlamnad, Avbokad }
public sealed class Bokning
{
    public Guid Id { get; } = Guid.NewGuid();
    public Kund Kund { get; }
    public Fordon Fordon { get; }
    public Hyrperiod Period { get; }
    public IReadOnlyList<Tillagg> Tillagg { get; }
    public decimal Totalpris { get; }
    public Bokningsstatus Status { get; private set; } = Bokningsstatus.Bokad;
    internal Bokning(Kund kund, Fordon bil, Hyrperiod period, IEnumerable<Tillagg> tillagg, decimal pris)
    { Kund = kund; Fordon = bil; Period = period; Tillagg = Array.AsReadOnly(tillagg.ToArray()); Totalpris = pris; }
    internal void Avboka() { Krav(Bokningsstatus.Bokad); Status = Bokningsstatus.Avbokad; }
    internal void HamtaUt() { Krav(Bokningsstatus.Bokad); Status = Bokningsstatus.Uthamtad; }
    internal void Aterlamna() { Krav(Bokningsstatus.Uthamtad); Status = Bokningsstatus.Aterlamnad; }
    private void Krav(Bokningsstatus krav)
    { if (Status != krav) throw new InvalidOperationException($"Åtgärden kräver {krav}; aktuell status är {Status}."); }
}
