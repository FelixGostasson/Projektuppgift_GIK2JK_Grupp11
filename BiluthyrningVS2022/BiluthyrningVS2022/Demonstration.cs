namespace BiluthyrningVS2022;
public static class Demonstration
{
    public static void Kor()
    {
        var period = new Hyrperiod(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 17));
        var bil = new Kombifabrik().Skapa("DEMO01", "Demokombi", 700m);
        Console.WriteLine("DEMO 1: Strategy och Decorator (fristående)");
        IPrisstrategi pris = new Veckorabatt();
        Console.WriteLine($"Veckorabatt: {pris.Berakna(bil, period)} kr");
        pris = new Tillaggspris(pris, new Tillagg("Barnstol", 50m));
        Console.WriteLine($"Med barnstol: {pris.Berakna(bil, period)} kr");
        Console.WriteLine("DEMO 2: Insertion sort (fristående)");
        foreach (var f in Fordonssortering.Sortera(new Fordon[] { bil, new Elbil("DEMO02", "Demoelbil", 500m) }))
            Console.WriteLine($"{f.Modell}: {f.PrisPerDygn} kr/dygn");
        Console.WriteLine("DEMO 3: Integrerat flöde, egen demonstrationsdata");
        var motor = Exempeldata.Skapa();
        var ledig = motor.SokLediga(period)[0];
        var bokning = motor.Boka(new Kund("DEMO", "Demokund"), ledig.Registreringsnummer, period);
        Console.WriteLine($"Sök → boka: {bokning.Fordon.Modell}, {bokning.Totalpris} kr");
        try { motor.Boka(bokning.Kund, ledig.Registreringsnummer, period); }
        catch (InvalidOperationException) { Console.WriteLine("Dubbelbokning stoppad."); }
        motor.HamtaUt(bokning.Id); motor.Aterlamna(bokning.Id);
        Console.WriteLine($"Hämta ut → återlämna: {bokning.Status}");
        var nasta = new Hyrperiod(period.Slut, period.Slut.AddDays(1));
        Console.WriteLine($"Tillgängliga nästa period: {motor.SokLediga(nasta).Count}");
    }
}
