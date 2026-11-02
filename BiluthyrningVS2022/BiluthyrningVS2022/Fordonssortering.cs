namespace BiluthyrningVS2022;
// Stabil insertion sort, synlig för att kunna diskuteras vid demonstrationen.
public static class Fordonssortering
{
    public static List<Fordon> Sortera(IEnumerable<Fordon> fordon)
    {
        ArgumentNullException.ThrowIfNull(fordon);
        var resultat = fordon.ToList();
        if (resultat.Any(f => f is null)) throw new ArgumentException("Fordon får inte vara null.");
        for (int i = 1; i < resultat.Count; i++)
        {
            var aktuell = resultat[i];
            int j = i - 1;
            while (j >= 0 && resultat[j].PrisPerDygn > aktuell.PrisPerDygn)
            { resultat[j + 1] = resultat[j]; j--; }
            resultat[j + 1] = aktuell;
        }
        return resultat;
    }
}
