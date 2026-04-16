# Smart Study Planner

Webapplicatie voor studenten om hun studieplanning te beheren en inzicht te krijgen in taken en deadlines.

## Tech stack
- ASP.NET Web API (C#)
- SQL Server
- HTML, CSS, JavaScript

## Functionaliteiten (MVP)
- Taken toevoegen
- Planning bekijken
- Deadlines beheren
- Advies ontvangen

## Security
Er is rekening gehouden met security door o.a. input validatie en bescherming tegen XSS.

## Gitflow

Voor dit project wordt een vereenvoudigde Gitflow gebruikt:

- `main` → bevat stabiele, werkende versies van de applicatie  
- `develop` → bevat de laatste ontwikkelversie  
- `feature/*` → wordt gebruikt voor het ontwikkelen van nieuwe functionaliteiten  

Nieuwe features worden ontwikkeld in een aparte feature branch en via een Pull Request gemerged naar `develop`.  
Na controle en goedkeuring kunnen wijzigingen uiteindelijk naar `main` worden gemerged.

Deze werkwijze zorgt voor overzicht, gecontroleerde code reviews en een stabiele hoofdbranch.
