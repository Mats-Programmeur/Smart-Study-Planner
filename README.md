# Smart Study Planner – MVP

## Over dit project

De Smart Study Planner is een webapplicatie die studenten helpt bij het plannen en organiseren van hun studie.
In deze eerste MVP (Minimum Viable Product) ligt de focus op het beheren van taken en het opslaan van gegevens via een API naar een database.

De applicatie toont een volledige datastroom:

* Invoer via de frontend (HTML/CSS/JavaScript)
* Verwerking via een ASP.NET Core API
* Opslag in een SQL Server database
* Terugkoppeling naar de gebruiker

---

## Functionaliteiten (MVP)

De huidige versie van de applicatie bevat de volgende functionaliteiten:

* ✅ Taak toevoegen
* ✅ Validatie van invoer (frontend + backend)
* ✅ Taken ophalen en weergeven
* ✅ Opslaan in database (Entity Framework + SQL Server)

### Ingevoerde gegevens:

Een taak bestaat uit:

* Titel (verplicht, max 120 tekens)
* Beschrijving (optioneel, max 500 tekens)
* Datum (niet verder dan 1 dag in het verleden)
* Starttijd
* Eindtijd (moet later zijn dan starttijd)

---

## Architectuur

De applicatie is opgebouwd volgens een eenvoudige 3-layer structuur:

### Frontend

* HTML (index.html)
* CSS (styles.css)
* JavaScript (app.js)

### Backend (API)

* ASP.NET Core Web API
* Controllers (TaskController)
* Services (TaskService)

### Database

* SQL Server (LocalDB)
* Entity Framework Core

---

## Dataflow

De gegevensstroom in de applicatie werkt als volgt:

1. De gebruiker vult een formulier in (frontend)
2. De frontend verstuurt een POST request naar `/tasks`
3. De API valideert en verwerkt de data
4. De data wordt opgeslagen in de database
5. De frontend haalt via GET `/tasks` de data weer op
6. De taken worden weergegeven in de UI

---

## Security – Mitigatie tegen XSS

### Dreiging

Een gebruiker (of aanvaller) kan kwaadaardige scripts invoeren via invoervelden, zoals de titel of beschrijving van een taak.
Wanneer deze invoer zonder filtering wordt opgeslagen en weergegeven, kan dit leiden tot een **Cross-Site Scripting (XSS)** aanval.

### Implementatie van mitigatie

In de backend wordt alle invoer gesanitized voordat deze wordt opgeslagen in de database.

Dit gebeurt in:
**TaskController.cs**

```csharp
var normalizedTitle = task.Titel?.Trim();
var normalizedDescription = task.Beschrijving?.Trim();

// XSS mitigatie
task.Titel = WebUtility.HtmlEncode(normalizedTitle);
task.Beschrijving = string.IsNullOrWhiteSpace(normalizedDescription)
    ? null
    : WebUtility.HtmlEncode(normalizedDescription);
```

### Resultaat

* Kwaadaardige scripts worden niet uitgevoerd
* Invoer wordt veilig opgeslagen
* Data wordt veilig weergegeven in de frontend

### Koppeling met threat model

Deze mitigatie heeft betrekking op:

* **STRIDE – Tampering**
* **STRIDE – Information Disclosure**

---

## Overige beveiligingsmaatregelen

Naast XSS-mitigatie zijn ook de volgende maatregelen toegepast:

* Input validatie in backend (verplicht veld, lengtecontrole, logische checks)
* Gebruik van Entity Framework (voorkomt SQL Injection)
* HTTPS redirect (in productie)
* Scheiding tussen frontend, API en database

---

## Installatie en gebruik

### Vereisten

* .NET 8+
* SQL Server LocalDB
* Visual Studio / Visual Studio Code

### Stappen

1. Clone de repository:

```
git clone <jouw-repo-url>
```

2. Open de oplossing in Visual Studio

3. Controleer de connection string in:

```
appsettings.json
```

4. Start de applicatie:

```
dotnet run
```

5. Open in browser:

```
https://localhost:<port>
```

---

## Projectstructuur

```
SmartStudyPlanner/
│
├── SmartStudyPlanner.Api/
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Data/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── index.html
│   └── Program.cs
```

---

## Toekomstige uitbreidingen

Mogelijke verbeteringen voor volgende iteraties:

*  Authenticatie (JWT)
*  Meerdere gebruikersrollen (student / beheerder)
*  Slim adviessysteem (AI/regels)
*  Kalenderweergave
*  Inzicht in studiegedrag

---

##  Auteur

Naam: Mats de Bont
Studentnummer: S1215102
Opleiding: HBO-ICT – Software Engineering

---

## 📎 Opmerking

Deze applicatie is ontwikkeld als onderdeel van een showcase-opdracht voor het vak Web Development.
De focus ligt op het aantonen van:

* gegevensopslag
* API communicatie
* en basis security (threat modelling)
