# Biluthyrning för Visual Studio 2022

Konsolprojekt i C#/.NET 8 för biluthyrningsscenariot. Fyra fordonstyper, sökning med typfilter, bokning, tillägg, avbokning, uthämtning och återlämning. Priset sparas vid bokning och överlappande bokningar avvisas.

## Starta

Öppna `BiluthyrningVS2022.sln` i Visual Studio 2022 med .NET 8-stöd. Välj BiluthyrningVS2022 som startprojekt och tryck Ctrl+F5. Lösningen innehåller även en separat testkörare.

Från den här mappen:

```text
dotnet build BiluthyrningVS2022.sln --configuration Release
dotnet run --project BiluthyrningVS2022
dotnet run --project BiluthyrningVS2022 -- --demo
dotnet run --project Biluthyrning.Tests --configuration Release
```

Datum anges som yyyy-MM-dd. Slutdatum är exklusivt: 10–13 november är tre dygn. Data finns endast i minnet och programmet stöder en användare åt gången. Välj j eller n för tilläggen. Boknings-ID visas när bokningen skapas och används vid statusändringar.

## Underlag

- [Krav, User Stories och Use Cases](docs/krav.md)
- [Domänmodell, APIE, SOLID, GRASP, mönster och algoritmer](docs/design.md)
- [Use Case-diagram i UML/PlantUML](docs/use-cases.puml)
- [Klassdiagram i UML/PlantUML](docs/klassdiagram.puml)
- [Test- och redovisningsinstruktioner](docs/redovisning.md)
- [Spårning mot issues](docs/issues.md)

PlantUML-filerna är redigerbara diagramkällor. Rendera dem med PlantUML före slutlig presentation. Gruppen behöver granska antaganden och UML samt komplettera med PowerPoint/PDF, egen reflektion och bekräftad åtkomst till repo/Projects för grupp och lärare. Underlaget är ingen garanti för ett visst betyg.
