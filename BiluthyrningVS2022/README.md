# Biluthyrning – utkast för Visual Studio 2022

Öppna `BiluthyrningVS2022.sln` i Visual Studio 2022 med stöd för .NET 8 och kör med Ctrl+F5.

Alternativt: `dotnet run --project BiluthyrningVS2022/BiluthyrningVS2022.csproj` från denna mapp.

## Funktioner

- Sök lediga fordon för en period, sorterade efter dygnspris.
- Boka ett fordon med kunduppgifter och valfri barnstol (50 kr/dygn).
- Visa bokningar och avboka via boknings-ID.
- Stoppa överlappande bokningar och hantera ogiltig inmatning.

## Avgränsningar och antaganden

Data lagras i minnet och försvinner när programmet avslutas. Slutdatum är exklusivt: 10–13 november är tre dygn, och nästa bokning kan börja den 13 november. Utkastet är avsett för en enkeltrådad konsolapp.

Prisberäkningen använder Strategy via `IPrisstrategi`. Utkastet är inte en komplett inlämning; bland annat återstår uppdelning i separata klassfiler, fler tester, ytterligare designmönster och dokumentation/UML som motsvarar koden.

## Kontrollerat

Projektet byggdes i Visual Studio 2022. Bokning med barnstol, avvisad dubbelbokning, tillgänglighet vid periodgräns och ogiltigt menyval kontrollerades.
