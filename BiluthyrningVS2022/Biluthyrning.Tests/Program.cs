using BiluthyrningVS2022;

var tester = new (string Namn, Action Test)[]
{
    ("Ogiltiga perioder avvisas", () => { Fel<ArgumentException>(() => new Hyrperiod(D(10), D(10))); Fel<ArgumentException>(() => new Hyrperiod(D(12), D(10))); }),
    ("Överlappning i båda riktningar och gräns", () =>
    {
        var p = P();
        Sant(p.Overlappar(new Hyrperiod(D(9), D(11))));
        Sant(p.Overlappar(new Hyrperiod(D(12), D(14))));
        Sant(p.Overlappar(new Hyrperiod(D(11), D(12))));
        Sant(p.Overlappar(new Hyrperiod(D(9), D(14))));
        Sant(!p.Overlappar(new Hyrperiod(D(13), D(14))));
        Sant(!p.Overlappar(new Hyrperiod(D(9), D(10))));
    }),
    ("Dubbelbokning och angränsande bokning", () =>
    {
        var m = Exempeldata.Skapa(); m.Boka(K(), "ABC123", P());
        Fel<InvalidOperationException>(() => m.Boka(K(), "ABC123", new Hyrperiod(D(11), D(12))));
        m.Boka(K(), "ABC123", new Hyrperiod(D(13), D(14)));
        Sant(m.Bokningar.Count == 2);
    }),
    ("Avbokning frigör och kan inte upprepas", () =>
    {
        var m = Exempeldata.Skapa(); var b = m.Boka(K(), "ABC123", P()); m.Avboka(b.Id);
        Sant(m.SokLediga(P()).Count == 4); m.Boka(K(), "ABC123", P());
        Fel<InvalidOperationException>(() => m.Avboka(b.Id));
    }),
    ("Tillåtna och otillåtna statusövergångar", () =>
    {
        var m = Exempeldata.Skapa(); var b = m.Boka(K(), "ABC123", P());
        Fel<InvalidOperationException>(() => m.Aterlamna(b.Id));
        m.HamtaUt(b.Id); Fel<InvalidOperationException>(() => m.Avboka(b.Id));
        Fel<InvalidOperationException>(() => m.HamtaUt(b.Id)); m.Aterlamna(b.Id);
        Sant(b.Status == Bokningsstatus.Aterlamnad);
        Fel<InvalidOperationException>(() => m.Aterlamna(b.Id));
        Sant(!m.SokLediga(P()).Any(f => f.Registreringsnummer == "ABC123"));
    }),
    ("Pris med flera tillägg och kopierad lista", () =>
    {
        var m = Exempeldata.Skapa(); var t = new List<Tillagg> { new("Barnstol", 50), new("Extra förare", 100) };
        var b = m.Boka(K(), "ABC123", P(), t); t.Clear();
        Sant(b.Totalpris == 2550m && b.Tillagg.Count == 2);
    }),
    ("Strategy och Decorator kombineras", () =>
    {
        IPrisstrategi s = new Tillaggspris(new Veckorabatt(), new Tillagg("Barnstol", 50));
        Sant(s.Berakna(new Kombi("A", "Kombi", 700), new Hyrperiod(D(10), D(17))) == 4760m);
        var m = new Bokningsmotor(new Veckorabatt()); m.LaggTill(new Kombi("A", "Kombi", 700));
        Sant(m.Boka(K(), "A", new Hyrperiod(D(10), D(17))).Totalpris == 4410m);
    }),
    ("Sökfilter och skiftlägesoberoende registrering", () =>
    {
        var m = Exempeldata.Skapa(); Sant(m.SokLediga(P(), " elbil ").Single() is Elbil);
        m.Boka(K(), " abc123 ", P()); Sant(m.SokLediga(P()).Count == 3);
        Sant(m.SokLediga(P(), "okänd").Count == 0);
    }),
    ("Stabil sortering, tom och redan sorterad lista", () =>
    {
        var a = new Kompaktbil("A", "A", 100); var b = new Kombi("B", "B", 100); var c = new Elbil("C", "C", 50);
        var original = new Fordon[] { a, b, c }; var sorterad = Fordonssortering.Sortera(original);
        Sant(sorterad.SequenceEqual(new Fordon[] { c, a, b })); Sant(original[0] == a);
        Sant(Fordonssortering.Sortera(sorterad).SequenceEqual(sorterad));
        Sant(Fordonssortering.Sortera(Array.Empty<Fordon>()).Count == 0);
    }),
    ("Alla fabriker producerar utbytbara fordon", () =>
    {
        Fordonsfabrik[] fabriker = { new Kompaktfabrik(), new Kombifabrik(), new Elbilsfabrik(), new Skapbilsfabrik() };
        var m = new Bokningsmotor(new Dygnspris());
        for (int i = 0; i < fabriker.Length; i++) { var f = fabriker[i].Skapa("F" + i, "Demo", 500); m.LaggTill(f); Sant(m.Boka(K(), f.Registreringsnummer, P()).Totalpris == 1500); }
    }),
    ("Ogiltiga domänvärden och dubletter", () =>
    {
        Fel<ArgumentException>(() => new Kund("", "Namn")); Fel<ArgumentException>(() => new Kund("K", ""));
        Fel<ArgumentOutOfRangeException>(() => new Kombi("A", "A", 0));
        Fel<ArgumentOutOfRangeException>(() => new Tillagg("Barnstol", -1));
        var m = Exempeldata.Skapa(); Fel<InvalidOperationException>(() => m.LaggTill(new Kombi("abc123", "Annan", 100)));
        Fel<ArgumentException>(() => m.Boka(K(), "ABC123", P(), new[] { new Tillagg("X", 1), new Tillagg("x", 2) }));
        Fel<ArgumentException>(() => m.Boka(K(), "ABC123", P(), new Tillagg[] { null! }));
        Sant(m.Bokningar.Count == 0);
    }),
    ("Okänt fordon och ID muterar inte data", () =>
    {
        var m = Exempeldata.Skapa(); Fel<KeyNotFoundException>(() => m.Boka(K(), "SAKNAS", P()));
        Fel<KeyNotFoundException>(() => m.Avboka(Guid.NewGuid())); Sant(m.Bokningar.Count == 0);
    }),
    ("Negativ prisstrategi får inte skapa bokning", () =>
    {
        var m = new Bokningsmotor(new NegativtPris()); m.LaggTill(new Kombi("A", "A", 100));
        Fel<InvalidOperationException>(() => m.Boka(K(), "A", P())); Sant(m.Bokningar.Count == 0);
    })
};
int misslyckade = 0;
foreach (var (namn, test) in tester)
{
    try { test(); Console.WriteLine("PASS: " + namn); }
    catch (Exception e) { misslyckade++; Console.WriteLine("FAIL: " + namn + ": " + e.Message); }
}
Console.WriteLine($"{tester.Length - misslyckade}/{tester.Length} tester godkända.");
return misslyckade == 0 ? 0 : 1;
static DateOnly D(int dag) => new(2026, 11, dag);
static Hyrperiod P() => new(D(10), D(13));
static Kund K() => new("K1", "Testkund");
static void Sant(bool villkor) { if (!villkor) throw new Exception("Villkoret uppfylldes inte."); }
static void Fel<T>(Action handling) where T : Exception
{
    try { handling(); } catch (T) { return; }
    throw new Exception("Förväntade " + typeof(T).Name);
}
sealed class NegativtPris : IPrisstrategi
{ public decimal Berakna(Fordon fordon, Hyrperiod period) => -1; }
