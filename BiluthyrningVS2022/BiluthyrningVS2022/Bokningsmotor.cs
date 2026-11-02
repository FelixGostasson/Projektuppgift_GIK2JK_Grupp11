namespace BiluthyrningVS2022;
// GRASP Controller: samordnar; Bokning äger status och Hyrperiod äger överlappsregeln.
public sealed class Bokningsmotor
{
    private readonly Dictionary<string, Fordon> _fordon = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, Bokning> _bokningar = new();
    private readonly IPrisstrategi _pris;
    public IReadOnlyList<Bokning> Bokningar => _bokningar.Values.ToList().AsReadOnly();
    public Bokningsmotor(IPrisstrategi pris) => _pris = pris ?? throw new ArgumentNullException(nameof(pris));
    public void LaggTill(Fordon fordon)
    {
        ArgumentNullException.ThrowIfNull(fordon);
        if (!_fordon.TryAdd(fordon.Registreringsnummer, fordon)) throw new InvalidOperationException("Registreringsnumret finns redan.");
    }
    private bool ArLedigt(Fordon fordon, Hyrperiod period) => !_bokningar.Values.Any(b =>
        b.Status != Bokningsstatus.Avbokad && b.Fordon == fordon && b.Period.Overlappar(period));
    public IReadOnlyList<Fordon> SokLediga(Hyrperiod period, string? typ = null)
    {
        ArgumentNullException.ThrowIfNull(period);
        var lediga = new List<Fordon>();
        foreach (var f in _fordon.Values)
            if ((string.IsNullOrWhiteSpace(typ) || f.Typ.Equals(typ.Trim(), StringComparison.OrdinalIgnoreCase)) && ArLedigt(f, period))
                lediga.Add(f);
        return Fordonssortering.Sortera(lediga).AsReadOnly();
    }
    public Bokning Boka(Kund kund, string reg, Hyrperiod period, IEnumerable<Tillagg>? tillagg = null)
    {
        ArgumentNullException.ThrowIfNull(kund);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentException.ThrowIfNullOrWhiteSpace(reg);
        if (!_fordon.TryGetValue(reg.Trim(), out var bil)) throw new KeyNotFoundException("Fordonet finns inte.");
        if (!ArLedigt(bil, period)) throw new InvalidOperationException("Fordonet är redan bokat under perioden.");
        var valda = (tillagg ?? Array.Empty<Tillagg>()).ToArray();
        if (valda.Any(t => t is null)) throw new ArgumentException("Ett tillägg får inte vara null.");
        if (valda.Select(t => t.Namn).Distinct(StringComparer.OrdinalIgnoreCase).Count() != valda.Length)
            throw new ArgumentException("Samma tillägg får bara väljas en gång.");
        IPrisstrategi pris = _pris;
        foreach (var t in valda) pris = new Tillaggspris(pris, t);
        var total = decimal.Round(pris.Berakna(bil, period), 2, MidpointRounding.AwayFromZero);
        if (total < 0) throw new InvalidOperationException("Priset får inte vara negativt.");
        var b = new Bokning(kund, bil, period, valda, total);
        _bokningar.Add(b.Id, b);
        return b;
    }
    private Bokning Hitta(Guid id) => _bokningar.TryGetValue(id, out var b) ? b : throw new KeyNotFoundException("Bokningen finns inte.");
    public void Avboka(Guid id) => Hitta(id).Avboka();
    public void HamtaUt(Guid id) => Hitta(id).HamtaUt();
    public void Aterlamna(Guid id) => Hitta(id).Aterlamna();
}
