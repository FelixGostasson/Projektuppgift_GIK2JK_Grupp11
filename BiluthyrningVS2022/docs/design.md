# Systemdesign och tekniska val

## Domän och ansvar

| Klass | Information och ansvar |
|---|---|
| Fordon | Oföränderligt registreringsnummer, modell och dygnspris; polymorf Typ |
| Kompaktbil, Kombi, Elbil, Skapbil | Konkreta fordonstyper med samma kontrakt |
| Kund | Kund-ID och namn på bokningen |
| Hyrperiod | Validerat start/slut, antal dygn och överlappsregeln |
| Tillagg | Namn och dygnspris för ett tillägg |
| Bokning | ID, kund, fordon, period, tillägg, pris och tillåtna statusövergångar |
| Bokningsmotor | Samordnar sökning, skapande och statusåtgärder; äger samlingarna |
| IPrisstrategi | Abstraktion för prisberäkning |
| Fordonsfabrik | Skapande via polymorf fabriksmetod |
| Fordonssortering | Stabil insertion sort efter dygnspris |

Klassdiagram med relationer och multiplicitet finns i [klassdiagram.puml](klassdiagram.puml). Program, Exempeldata och Demonstration är start-/presentationskod, inte domänobjekt. Dokumentet beskriver den implementerade modellen som ett förslag för gruppens granskning.

## APIE

Abstraktion: Fordon och IPrisstrategi ger gemensamma kontrakt. Polymorfism: bokningsmotorn hanterar samtliga Fordon utan typkontroll och anropar injicerad prisstrategi. Inheritance: konkreta fordon och fabriker ärver basansvar. Encapsulation: egenskaper är skrivskyddade, status har privat setter och kan bara ändras via interna metoder med validering. Samlingar exponeras som skrivskyddade kopior eller vyer.

I ett integrerat användningsfall skapar Kombifabrik en Kombi, motorn tar emot den som Fordon, använder IPrisstrategi och skapar en Bokning vars status styrs av objektet självt. Fordonssubklasserna har små beteendeskillnader idag. En typkod/enum vore enklare om typer aldrig får egna beteenden; arvet visar utbytbarhet men kostar fler klasser.

## SOLID och GRASP

SRP: Program läser/skriver, Hyrperiod validerar intervall, prisstrategier räknar pris, Bokning äger status. Att lägga allt i Program hade gett färre filer men sämre testbarhet.

OCP och DIP: Bokningsmotor beror på IPrisstrategi. Nya strategier och nya Tillagg-värden kan användas utan ändring i motorn. Menyn måste fortfarande utökas om nya val ska visas. En switch i motorn vore enklare initialt men skulle samla allt fler prisregler där.

LSP: alla Fordon har samma positiva pris och kan bokas med samma logik; ingen subklass kräver specialfall. ISP: prisinterfacet har bara en operation.

GRASP Controller: Bokningsmotor samordnar användningsfallen. Information Expert: Hyrperiod vet hur två intervall överlappar; Bokning vet sin status. Creator: motorn äger och skapar bokningar. Pure Fabrication: prisstrategier och sorteringshjälpare håller domänobjekten fokuserade. Low Coupling/High Cohesion: domänen känner inte konsolen; prissättningen beror på ett litet kontrakt.

## Tre designmönster

1. Strategy: Dygnspris och Veckorabatt implementerar IPrisstrategi. Val sker i konstruktionen av Bokningsmotor; ordinarie meny använder Dygnspris. Ett nytt prisbeteende kräver en ny implementation, inte ändring i motorn. Nackdel: fler små klasser och konfiguration.
2. Decorator: Tillaggspris implementerar samma interface och omsluter en annan IPrisstrategi. Motorn bygger en kedja för valda tillägg. Grundhyrans rabatt beräknas innerst, tillägg läggs sedan ovanpå. Alternativet är en summa i en enda prisfunktion, enklare men mindre flexibel för framtida tilläggsbeteenden.
3. Factory Method: Fordonsfabrik.Skapa anropar den skyddade abstrakta SkapaFordon. Konkreta fabriker överskuggar metoden. Exempeldata konfigurerar rätt fabrik; motorn behöver inte känna fordonens konkreta konstruktörer. Direkt new vore rimligt för denna lilla flotta och fabrikerna tillför främst värde om skapandereglerna växer.

Mönstren samverkar när en fabrik skapar fordonet, motorn använder en Strategy och tillägg omsluter strategin genom Decorator. State-mönstret används inte: fyra enkla statusvärden och kontrollerade övergångar räcker. Lägg inte till State enbart för att få ytterligare ett mönster.

## Datastrukturer och algoritmer

F = antal fordon, B = antal bokningar, L = antal lediga fordon, T = valda tillägg.

- Dictionary<string,Fordon> ger förväntat O(1) uppslag med registreringsnummer och avvisar dubletter. StringComparer.OrdinalIgnoreCase gör uppslag oberoende av skiftläge. En List skulle ge O(F) för varje uppslag, men lägre konceptuell kostnad.
- Dictionary<Guid,Bokning> ger förväntat O(1) ID-uppslag för statusåtgärder. Dictionary har O(n) värsta fall vid dålig hashfördelning. Att visa alla bokningar skapar en kopia i O(B) tid/minne.
- Tillgänglighet söker linjärt genom bokningarna och avbryter vid första konflikt. En enskild kontroll är O(B), hela sökningen O(F*B). Insertion sort tillkommer med O(L²) värsta/genomsnitt och O(L) bästa fall på redan sorterade resultat. Total sökning O(F*B + L²), extra minne O(L). Varje intervalljämförelse är O(1).
- Insertion sort är stabil eftersom endast strikt större pris flyttas; lika priser bevarar indatans ordning. Resultatet kopieras så att indata inte ändras. Dictionary-ordning används inte som ett verksamhetslöfte för lika priser.
- Bokning kontrollerar överlapp i O(B) och bygger T dekoratorer i O(T). Kontroll av unika tillägg använder Distinct, förväntat O(T). Lagring av bokningen är förväntat O(1).

För en liten undervisningsflotta är implementationen lätt att förklara och testa. För tusentals fordon vore List.Sort/OrderBy med O(L log L) och bokningar indexerade per fordon mer rimligt. Intervallträd ger effektivare överlappsfrågor men är onödigt komplext här. Det nuvarande skyddet är inte trådsäkert; en riktig flertjänstlösning behöver transaktion eller lås kring kontroll och lagring.

## Regler för tillgänglighet

Överlapp iff a.Start < b.Slut och b.Start < a.Slut. Samma period, innesluten period och delvis överlapp avvisas. [10,13) och [13,14) tillåts. Avbokad ignoreras. Återlämnad behålls som historik och blockerar ursprungligt intervall. Ny period efter slutet tillåts. Tester täcker dessa exempel.

## Reflektion och kvarstående arbete

Det första utkastets stora Program-fil har delats i domän, prissättning, skapande, samordning och presentation. Barnstolens bool-värde har ersatts av en samling generella tillägg. Valet gör kombinationer möjliga men ökar antalet klasser. In-memory-lagring och manuella statusåtgärder är avgränsningar, inte produktionslösningar. Gruppen ska komplettera med sina egna erfarenheter och motiveringar; inga möten eller individuella bidrag påstås här.

Före inlämning behöver gruppen granska krav/antaganden, verifiera GitHub Projects och alla medlemmars/lärarnas åtkomst, skapa den slutliga PowerPoint/PDF-presentationen och öva redovisningen. Se redovisning.md. Detta kodunderlag är ingen garanti för ett visst betyg.
