using System.Globalization;
namespace BiluthyrningVS2022;
internal static class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if (args.Contains("--demo")) { Demonstration.Kor(); return; }
        var motor = Exempeldata.Skapa();
        try
        {
            while (true)
            {
                Console.WriteLine("\nBILUTHYRNING\n1. Sök lediga fordon\n2. Boka\n3. Visa bokningar\n4. Avboka\n5. Hämta ut\n6. Återlämna\n7. Demonstrationer\n0. Avsluta");
                Console.Write("Val: ");
                var val = Console.ReadLine();
                if (val is null or "0") return;
                try
                {
                    switch (val)
                    {
                        case "1":
                            var period = LasPeriod();
                            Console.Write("Fordonstyp (tomt = alla): ");
                            var lediga = motor.SokLediga(period, Console.ReadLine());
                            foreach (var f in lediga) Console.WriteLine($"{f.Registreringsnummer} | {f.Modell} | {f.Typ} | {f.PrisPerDygn} kr/dygn");
                            if (lediga.Count == 0) Console.WriteLine("Inga lediga fordon.");
                            break;
                        case "2":
                            var hyrperiod = LasPeriod();
                            var reg = LasText("Registreringsnummer: ");
                            var kund = new Kund(LasText("Kund-ID: "), LasText("Namn: "));
                            var tillagg = new List<Tillagg>();
                            if (LasJaNej("Barnstol (50 kr/dygn)?")) tillagg.Add(new Tillagg("Barnstol", 50m));
                            if (LasJaNej("Extra förare (100 kr/dygn)?")) tillagg.Add(new Tillagg("Extra förare", 100m));
                            Visa(motor.Boka(kund, reg, hyrperiod, tillagg));
                            break;
                        case "3":
                            foreach (var b in motor.Bokningar) Visa(b);
                            if (motor.Bokningar.Count == 0) Console.WriteLine("Inga bokningar.");
                            break;
                        case "4": motor.Avboka(LasId()); Console.WriteLine("Avbokad."); break;
                        case "5": motor.HamtaUt(LasId()); Console.WriteLine("Uthämtad."); break;
                        case "6": motor.Aterlamna(LasId()); Console.WriteLine("Återlämnad."); break;
                        case "7": Demonstration.Kor(); break;
                        default: Console.WriteLine("Välj ett alternativ mellan 0 och 7."); break;
                    }
                }
                catch (Exception fel) when (fel is ArgumentException or InvalidOperationException or KeyNotFoundException)
                { Console.WriteLine($"Kunde inte genomföra: {fel.Message}"); }
            }
        }
        catch (EndOfStreamException) { Console.WriteLine("\nInmatningen avslutades."); }
    }
    private static void Visa(Bokning b) => Console.WriteLine(
        $"{b.Id} | {b.Kund.Namn} | {b.Fordon.Registreringsnummer} | {b.Period.Start:yyyy-MM-dd}–{b.Period.Slut:yyyy-MM-dd} | {b.Totalpris} kr | {b.Status} | Tillägg: {string.Join(", ", b.Tillagg.Select(t => t.Namn))}");
    private static string LasText(string fraga)
    {
        Console.Write(fraga);
        var text = Console.ReadLine() ?? throw new EndOfStreamException();
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Fältet får inte vara tomt.");
        return text.Trim();
    }
    private static bool LasJaNej(string fraga)
    {
        while (true)
        {
            var svar = LasText(fraga + " (j/n): ").ToLowerInvariant();
            if (svar == "j") return true;
            if (svar == "n") return false;
            Console.WriteLine("Svara j eller n.");
        }
    }
    private static Guid LasId() => Guid.TryParse(LasText("Boknings-ID: "), out var id)
        ? id : throw new ArgumentException("Ogiltigt boknings-ID.");
    private static Hyrperiod LasPeriod() => new(LasDatum("Start (yyyy-MM-dd): "), LasDatum("Slut, exklusivt (yyyy-MM-dd): "));
    private static DateOnly LasDatum(string fraga) => DateOnly.TryParseExact(LasText(fraga),
        "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datum)
        ? datum : throw new ArgumentException("Datum anges som yyyy-MM-dd.");
}
