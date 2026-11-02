# Demonstration och redovisningsunderlag

## Körning

Öppna lösningen i Visual Studio 2022, välj BiluthyrningVS2022 som startprojekt och tryck Ctrl+F5. Meny 7 visar två fristående demonstrationer och ett integrerat flöde. Alternativ från projektmappen: `dotnet run --project BiluthyrningVS2022 -- --demo`.

1. Strategy/Decorator: grundhyra 700 × 7 = 4 900 kr, veckorabatt ger 4 410 kr; barnstol ger totalt 4 760 kr. Visa kedjan i Prissattning.cs och diskutera rabattens ordning.
2. Algoritm: osorterade demoobjekt med dygnspris 700 och 500 ger 500, 700. Visa while-loopen och diskutera stabilitet och O(n²).
3. Integrerat: sök → boka billigaste fordon → avvisa dubbelbokning → hämta ut → återlämna → sök nästa angränsande period. Demo använder egen motor och påverkar inte menydata.
4. Manuell avbokning: boka i meny 2, kopiera det skapade ID:t och använd meny 4. Sök samma period: bilen ska åter synas. Försök avboka igen: tydligt fel, ingen krasch.

## Tester

`dotnet run --project Biluthyrning.Tests --configuration Release` kör den fristående testköraren. Den använder inga externa testpaket. Den skriver PASS/FAIL och returnerar exitkod 1 vid fel; den visas därför inte som MSTest/xUnit i Test Explorer och ska inte köras med `dotnet test` som enda kontroll.

Tester täcker giltiga/ogiltiga perioder, olika överlapp, angränsning, dubbelbokning, avbokning och återbokning, statusövergångar, historik, sammansatt pris, oföränderliga tillägg, strategiutbyte, filter, fabriker, stabil sortering och felaktiga domänvärden. Konsolflödet kontrolleras separat med styrd inmatning.

## Förslag till presentation på högst 20 minuter

| Tid | Innehåll |
|---|---|
| 0–2 min | Kundproblem, avgränsningar och antaganden |
| 2–5 min | User Stories, Use Cases och diagram |
| 5–8 min | Domänmodell, APIE, SRP/DIP och GRASP |
| 8–11 min | Strategy, Decorator och Factory Method; alternativ och nackdelar |
| 11–13 min | Dictionary, sökning, insertion sort och komplexitet |
| 13–18 min | Två fristående demonstrationer och integrerat scenario |
| 18–20 min | Reflektion, begränsningar och frågor |

Detta är ett manusunderlag. Den PowerPoint/PDF som uppgiften efterfrågar återstår att skapa och granska. Fördela talartid inom gruppen och låt alla kunna förklara kontrakt, tillgänglighetsregel och mönstrens samspel.
