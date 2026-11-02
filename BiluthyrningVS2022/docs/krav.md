# Kravbild och användningsfall

Utgångspunkt är Uppdrag 6 i Projektuppgift GIK2JK. Kunden behöver en konsolbaserad bokningstjänst som söker lediga fordon, bokar och avbokar samt räknar ut pris. Detta underlag beskriver implementationen, inte genomförda kundmöten eller gruppbeslut. Gruppen behöver granska antagandena med kunden.

## Avgränsningar och antaganden

- C#/.NET 8, Visual Studio 2022, ingen databas, webb, extern tjänst eller inloggning.
- Data finns i minnet och återställs vid omstart. En användare i taget; samtidiga bokningar stöds inte.
- Perioden är [start, slut). 10–13 november innebär tre dygn. Nästa uthyrning får börja den 13 november.
- Historiska datum tillåts för demonstration. Klockslag, körkortsbedömning, betalning, skador och förseningsavgifter ingår inte.
- Återlämning ändrar status men inte avtalad period eller pris. En återlämnad bokning behålls i historiken och blockerar sitt ursprungliga intervall. Tidig återlämning frigör inte resterande avtalsperiod.
- Uthämtning och återlämning sker via manuell konsolåtgärd utan kontroll mot dagens datum.
- Endast bokade bokningar får avbokas. Uthämtad får bara återlämnas; återlämnad och avbokad är slutstatus.
- Barnstol kostar 50 kr/dygn, extra förare 100 kr/dygn. Varje tillägg får förekomma en gång. Priser är exempelvärden, inte en avtalad prislista.
- Ordinarie meny använder Dygnspris. Veckorabatt, 10 procent från sju dygn på grundhyran, visas i demo och test; tillägg rabatteras inte.
- Kund-ID är en etikett på bokningen, ingen gemensam kunddatabas. Samma ID garanterar inte samma namn mellan olika bokningar.

## Krav och spårning

| ID | Krav | Implementation / verifiering | Issue |
|---|---|---|---|
| F1 | Sök fordon för giltig period och valfri typ | SokLediga, filter- och periodtester | #11, #12 |
| F2 | Boka och koppla kund, fordon och period | Boka, Bokning; bokningstester | #7, #11 |
| F3 | Förhindra överlappande bokningar | Hyrperiod.Overlappar, ArLedigt | #12 |
| F4 | Beräkna pris från fordon, dygn och tillägg | IPrisstrategi, Tillaggspris | #10, #11 |
| F5 | Avboka och frigör fordon | Avboka, statusövergångstest | #5, #11 |
| F6 | Visa bokningar med ID, status och pris | Meny 3 | #5 |
| F7 | Hämta ut och återlämna | Meny 5–6, statusövergångstest | #5 |
| F8 | Fyra fordonstyper och sortering efter pris | Fordonssubklasser, insertion sort | #7, #10 |
| NF1 | Felaktiga datum, ID och menyval hanteras | TryParse, domänvalidering, felmeddelanden | #9, #12 |
| NF2 | Bokningslogik kan testas utan konsol | Separat testprojekt, injicerad prisstrategi | #9 |
| NF3 | Nya prisregler/tillägg utan ändring i motorn | Strategy och Decorator | #10, #11 |
| NF4 | Prestanda rimlig för liten flotta | Dictionary, linjär sökning, insertion sort; analys i design.md | #9 |

## User Stories och acceptanskriterier

1. Som kund vill jag se lediga fordon för min period för att välja en passande bil. Givet en överlappande aktiv bokning ska bilen saknas i sökresultatet; angränsande period ska vara möjlig. Typfilter ska fungera oberoende av skiftläge och resultatet ska sorteras efter pris.
2. Som kund vill jag boka en bil med tillägg för att veta hyrans totalpris. Givet en kombi för 700 kr/dygn i tre dygn med barnstol och extra förare ska priset bli 2 550 kr. Bokningen ska få unikt ID och status Bokad. Okänt fordon eller dubbelbokning ska avvisas utan ny bokning.
3. Som kund vill jag avboka innan uthämtning så att bilen blir ledig. Givet Bokad ska status bli Avbokad och perioden frigöras. Upprepad avbokning och avbokning efter uthämtning ska avvisas.
4. Som personal vill jag se bokningarna för att hantera uthämtning. Varje rad ska visa ID, kund, fordon, period, totalpris, tillägg och status.
5. Som personal vill jag hämta ut och återlämna ett bokat fordon. Endast Bokad → Uthamtad → Aterlamnad tillåts; direkt återlämning från Bokad ska ge fel.

## UC1 Sök lediga fordon

Aktör: kund, via personal/konsol. Förutsättning: fordonsflottan är inlagd. Huvudflöde: välj sök, ange start och slut samt valfri typ; systemet validerar intervallet, filtrerar lediga och sorterar efter dygnspris. Resultat: lediga fordon visas utan att data ändras. Alternativ: ogiltigt datum eller slut före start ger fel; inga träffar ger ett tydligt meddelande.

## UC2 Boka fordon

Aktör: kund. Förutsättning: fordonet finns. Huvudflöde: ange period, registreringsnummer, kund-ID och namn; välj tillägg; systemet kontrollerar tillgänglighet, räknar pris och skapar bokning. Resultat: Bokad med ID och låst totalpris. Alternativ: okänt fordon, ogiltig period eller överlapp ger fel utan att boka. Ogiltigt j/n efterfrågas igen. Avbruten inmatningsström avslutar programmet.

## UC3 Avboka

Aktör: kund. Förutsättning: bokningen finns och är Bokad. Ange boknings-ID; systemet ändrar status till Avbokad. Resultat: bilen kan bokas under samma period. Alternativ: felaktigt/okänt ID eller annan status ger fel utan ändring.

## UC4 Hämta ut

Aktör: personal. Förutsättning: Bokad. Ange ID och välj uthämtning; systemet ändrar status till Uthamtad. Okänt ID eller annan status avvisas. Ingen fysisk betalnings- eller identitetskontroll simuleras.

## UC5 Återlämna

Aktör: personal. Förutsättning: Uthamtad. Ange ID; systemet ändrar status till Aterlamnad och bevarar pris och period. Direkt eller upprepad återlämning avvisas.

Use Case-diagram: [use-cases.puml](use-cases.puml). Kund och personal representerar verksamhetsroller; konsolen har ingen behörighetskontroll.

## Öppna kundfrågor

Behövs klockslag, förbud mot historiska bokningar, tidig återlämning, gemensamt kundregister och andra priser? Gruppen behöver besluta om dessa innan reglerna ändras. Dubbelbokningsskydd vid parallella användare skulle kräva annan lagrings- och transaktionslösning.
