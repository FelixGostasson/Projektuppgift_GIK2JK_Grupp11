namespace BiluthyrningVS2022;
// Factory Method: den gemensamma operationen anropar en överskuggad fabriksmetod.
public abstract class Fordonsfabrik
{
    public Fordon Skapa(string reg, string modell, decimal pris) => SkapaFordon(reg, modell, pris);
    protected abstract Fordon SkapaFordon(string reg, string modell, decimal pris);
}
public sealed class Kompaktfabrik : Fordonsfabrik
{ protected override Fordon SkapaFordon(string reg, string modell, decimal pris) => new Kompaktbil(reg, modell, pris); }
public sealed class Kombifabrik : Fordonsfabrik
{ protected override Fordon SkapaFordon(string reg, string modell, decimal pris) => new Kombi(reg, modell, pris); }
public sealed class Elbilsfabrik : Fordonsfabrik
{ protected override Fordon SkapaFordon(string reg, string modell, decimal pris) => new Elbil(reg, modell, pris); }
public sealed class Skapbilsfabrik : Fordonsfabrik
{ protected override Fordon SkapaFordon(string reg, string modell, decimal pris) => new Skapbil(reg, modell, pris); }
public static class Exempeldata
{
    public static Bokningsmotor Skapa()
    {
        var motor = new Bokningsmotor(new Dygnspris());
        motor.LaggTill(new Kombifabrik().Skapa("ABC123", "Volvo V60", 700m));
        motor.LaggTill(new Elbilsfabrik().Skapa("ELB456", "Kia Niro", 850m));
        motor.LaggTill(new Kompaktfabrik().Skapa("KMP789", "Toyota Yaris", 450m));
        motor.LaggTill(new Skapbilsfabrik().Skapa("SKP012", "Ford Transit", 950m));
        return motor;
    }
}
