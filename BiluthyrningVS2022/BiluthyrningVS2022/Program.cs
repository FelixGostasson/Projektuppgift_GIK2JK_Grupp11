using System.Globalization;

namespace BiluthyrningVS2022;

// Utkast: alla data lagras i minnet. Konsolen ansvarar bara for in- och utmatning.
internal static class Program
{
    static void Main()
    {
        var motor = new Bokningsmotor(new Dygnspris());
        motor.LaggTill(new Fordon("ABC123", "Volvo V60", "Kombi", 700m));
        motor.LaggTill(new Fordon("ELB456", "Kia Niro", "Elbil", 850m));
        motor.LaggTill(new Fordon("KMP789", "Toyota Yaris", "Kompakt", 450m));
        while (true)
        {
            Console.WriteLine("\nBILUTHYRNING\n1. Sok lediga fordon\n2. Boka\n3. Visa bokningar\n4. Avboka\n0. Avsluta");
            Console.Write("Val: ");
            var val = Console.ReadLine();
            if (val is null or "0") return;
            try
            {
                switch (val)
                {
                    case "1":
                        var lediga = motor.SokLediga(LasPeriod());
                        foreach (var bil in lediga)
                            Console.WriteLine($"{bil.Registreringsnummer} | {bil.Modell} | {bil.Typ} | {bil.PrisPerDygn} kr/dygn");
                        if (lediga.Count == 0) Console.WriteLine("Inga lediga fordon.");
                        break;
                    case "2":
                        var period = LasPeriod();
                        var reg = LasText("Registreringsnummer: ");
                        var kund = new Kund(LasText("Kund-ID: "), LasText("Namn: "));
                        Console.Write("Barnstol 50 kr/dygn? (j/n): ");
                        bool barnstol = Console.ReadLine()?.Trim().ToLowerInvariant() == "j";
                        var bokning = motor.Boka(kund, reg, period, barnstol);
                        Console.WriteLine($"Bokad! ID: {bokning.Id} | Totalpris: {bokning.Totalpris} kr");
                        break;
                    case "3":
                        foreach (var b in motor.Bokningar)
                            Console.WriteLine($"{b.Id} | {b.Kund.Namn} | {b.Fordon.Registreringsnummer} | {b.Period.Start:yyyy-MM-dd} - {b.Period.Slut:yyyy-MM-dd} | {b.Totalpris} kr | {(b.Avbokad ? "Avbokad" : "Bokad")}");
                        if (motor.Bokningar.Count == 0) Console.WriteLine("Inga bokningar.");
                        break;
                    case "4":
                        if (!Guid.TryParse(LasText("Boknings-ID: "), out var id))
                            throw new ArgumentException("Ange ett giltigt boknings-ID.");
                        motor.Avboka(id);
                        Console.WriteLine("Bokningen ar avbokad.");
                        break;
                    default: Console.WriteLine("Valj 0, 1, 2, 3 eller 4."); break;
                }
            }
            catch (Exception fel) when (fel is ArgumentException or InvalidOperationException or KeyNotFoundException)
            { Console.WriteLine($"Kunde inte genomfora: {fel.Message}"); }
        }
    }
    static string LasText(string fraga)
    {
        Console.Write(fraga);
        var text = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Faltet far inte vara tomt.");
        return text.Trim();
    }
    static Hyrperiod LasPeriod()
    {
        if (!DateOnly.TryParseExact(LasText("Start (yyyy-MM-dd): "), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
            !DateOnly.TryParseExact(LasText("Slut (yyyy-MM-dd): "), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var slut))
            throw new ArgumentException("Datum anges som yyyy-MM-dd.");
        return new Hyrperiod(start, slut);
    }
}

public sealed class Hyrperiod
{
    public DateOnly Start { get; }
    public DateOnly Slut { get; }
    public int AntalDygn => Slut.DayNumber - Start.DayNumber;
    public Hyrperiod(DateOnly start, DateOnly slut)
    {
        if (slut <= start) throw new ArgumentException("Slutdatum maste vara efter startdatum.");
        Start = start; Slut = slut;
    }
    // Slutdatum ar exklusivt. Bokningar som bara moter varandra overlappar inte.
    public bool Overlappar(Hyrperiod annan) => Start < annan.Slut && annan.Start < Slut;
}

public sealed class Fordon
{
    public string Registreringsnummer { get; }
    public string Modell { get; }
    public string Typ { get; }
    public decimal PrisPerDygn { get; }
    public Fordon(string reg, string modell, string typ, decimal pris)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reg);
        ArgumentException.ThrowIfNullOrWhiteSpace(modell);
        ArgumentException.ThrowIfNullOrWhiteSpace(typ);
        if (pris < 0) throw new ArgumentOutOfRangeException(nameof(pris));
        Registreringsnummer = reg.Trim().ToUpperInvariant();
        Modell = modell.Trim(); Typ = typ.Trim(); PrisPerDygn = pris;
    }
}
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

// Strategy separerar prisregeln fran bokningslogiken.
public interface IPrisstrategi
{
    decimal Berakna(Fordon fordon, Hyrperiod period, bool barnstol);
}
public sealed class Dygnspris : IPrisstrategi
{
    public decimal Berakna(Fordon fordon, Hyrperiod period, bool barnstol)
        => (fordon.PrisPerDygn + (barnstol ? 50m : 0m)) * period.AntalDygn;
}
public sealed class Bokning
{
    public Guid Id { get; } = Guid.NewGuid();
    public Kund Kund { get; }
    public Fordon Fordon { get; }
    public Hyrperiod Period { get; }
    public bool Barnstol { get; }
    public decimal Totalpris { get; }
    public bool Avbokad { get; private set; }
    internal Bokning(Kund kund, Fordon bil, Hyrperiod period, bool barnstol, decimal pris)
    { Kund = kund; Fordon = bil; Period = period; Barnstol = barnstol; Totalpris = pris; }
    internal void Avboka() => Avbokad = true;
}

// Samordnar anvandningsfallen. Utkastet ar avsett for en enkeltradad konsolapp.
public sealed class Bokningsmotor
{
    private readonly Dictionary<string, Fordon> _fordon = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Bokning> _bokningar = new();
    private readonly IPrisstrategi _pris;
    public IReadOnlyList<Bokning> Bokningar => _bokningar.AsReadOnly();
    public Bokningsmotor(IPrisstrategi pris)
    { _pris = pris ?? throw new ArgumentNullException(nameof(pris)); }
    public void LaggTill(Fordon bil)
    {
        ArgumentNullException.ThrowIfNull(bil);
        if (!_fordon.TryAdd(bil.Registreringsnummer, bil))
            throw new InvalidOperationException("Registreringsnumret finns redan.");
    }
    private bool ArLedigt(Fordon bil, Hyrperiod period) => !_bokningar.Any(b =>
        !b.Avbokad && b.Fordon == bil && b.Period.Overlappar(period));
    public IReadOnlyList<Fordon> SokLediga(Hyrperiod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return _fordon.Values.Where(f => ArLedigt(f, period)).OrderBy(f => f.PrisPerDygn).ToList().AsReadOnly();
    }
    public Bokning Boka(Kund kund, string reg, Hyrperiod period, bool barnstol)
    {
        ArgumentNullException.ThrowIfNull(kund);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentException.ThrowIfNullOrWhiteSpace(reg);
        if (!_fordon.TryGetValue(reg.Trim(), out var bil)) throw new KeyNotFoundException("Fordonet finns inte.");
        if (!ArLedigt(bil, period)) throw new InvalidOperationException("Fordonet ar redan bokat under perioden.");
        var pris = _pris.Berakna(bil, period, barnstol);
        if (pris < 0) throw new InvalidOperationException("Priset far inte vara negativt.");
        var bokning = new Bokning(kund, bil, period, barnstol, pris);
        _bokningar.Add(bokning);
        return bokning;
    }
    public void Avboka(Guid id)
    {
        var bokning = _bokningar.FirstOrDefault(b => b.Id == id) ?? throw new KeyNotFoundException("Bokningen finns inte.");
        if (bokning.Avbokad) throw new InvalidOperationException("Bokningen ar redan avbokad.");
        bokning.Avboka();
    }
}
