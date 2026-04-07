# AccessiTrack 🔍♿

> **Plateforme de suivi d'audits d'accessibilité web (WCAG 2.1/2.2)**  
> Développée pour démontrer la maîtrise de la Clean Architecture,  
> .NET 10, Angular et Azure DevOps.

---

## 🎯 Contexte du projet

Suite à un stage au **CRTC (Conseil de la radiodiffusion et des  
télécommunications canadiennes)** en programmation et accessibilité web,  
j'ai conçu AccessiTrack pour répondre à un besoin réel :  
**les équipes de développement n'ont pas d'outil simple pour suivre  
et résoudre les violations WCAG de leurs projets web.**

---

## 🌐 Démo en ligne

| | Lien |
|---|---|
| 🖥️ **Application** | `https://accessitrack.vercel.app` *(à venir)* |
| 📡 **API Swagger** | `https://accessitrack-api.azurewebsites.net/swagger` *(à venir)* |
| 🎥 **Vidéo de présentation** | `https://loom.com/...` *(à venir)* |
| 📋 **Azure DevOps Board** | [Voir le backlog Agile](https://dev.azure.com/charron-philippe-dev/AccessiTrack) |

---

## 🏗️ Architecture

Ce projet suit les principes de la **Clean Architecture** de Robert C. Martin.  
Les dépendances pointent toujours vers le centre (Domain).  
Aucune couche extérieure ne peut corrompre la logique métier.
```
┌─────────────────────────────────────────┐
│              Client Angular             │  ← Présentation UI
│         (TypeScript, ARIA, WCAG)        │
└─────────────────┬───────────────────────┘
                  │ HTTP/REST
┌─────────────────▼───────────────────────┐
│              API Layer                  │  ← Contrôleurs REST
│        (ASP.NET Core Web API)           │     Swagger / OpenAPI
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│          Application Layer              │  ← Cas d'utilisation
│      (MediatR CQRS, FluentValidation)   │     Commands / Queries
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│            Domain Layer                 │  ← Cœur métier pur
│   (Entités, Règles, Interfaces)         │     Aucune dépendance
└─────────────────────────────────────────┘
                  ▲
┌─────────────────┴───────────────────────┐
│         Infrastructure Layer            │  ← EF Core, SQL Server
│   (Repositories, DbContext, Migrations) │     Implémente les interfaces
└─────────────────────────────────────────┘
```

---

## ⚙️ Stack technologique

| Couche | Technologie | Version |
|--------|-------------|---------|
| **Domain** | C#, POCO Entities, Factory Methods | .NET 10 |
| **Application** | MediatR (CQRS), FluentValidation | Latest |
| **Infrastructure** | Entity Framework Core, SQL Server | EF Core 10 |
| **API** | ASP.NET Core Web API, Swagger/OpenAPI | .NET 10 |
| **Client** | Angular, TypeScript, Angular Material | Angular 18 |
| **Tests** | xUnit, Moq, FluentAssertions | Latest |
| **CI/CD** | Azure DevOps Pipelines (YAML) | - |
| **Hébergement API** | Azure App Service (Free Tier) | - |
| **Hébergement Client** | Vercel | - |
| **Base de données** | Azure SQL Database (Free Tier) | - |

---

## ♿ Accessibilité (spécialité CRTC)

L'interface Angular respecte les normes **WCAG 2.1 niveau AA** :

- ✅ Navigation 100% au clavier (Tab, Entrée, Échap)
- ✅ Annonces `aria-live` pour les confirmations de sauvegarde
- ✅ Ratio de contraste minimum **4.5:1** sur tous les textes
- ✅ Attributs `aria-label` sur tous les boutons et icônes
- ✅ Gestion du focus lors des changements de route (SPA)
- ✅ Compatible NVDA et JAWS (lecteurs d'écran)

---

## 🚀 Lancer le projet localement

### Prérequis
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org)
- [SQL Server](https://www.microsoft.com/sql-server) ou Docker

### Backend
```bash
cd Backend
dotnet restore AccessiTrack.sln
dotnet ef database update --project AccessiTrack.Infrastructure \
  --startup-project AccessiTrack.API
dotnet run --project AccessiTrack.API
# API disponible sur : https://localhost:7001/swagger
```

### Frontend
```bash
cd Frontend/accessitrack-client
npm install
ng serve
# Application disponible sur : http://localhost:4200
```

---

## 🔄 Pipeline CI/CD

Le pipeline Azure DevOps automatise :

1. **Build** → Compilation .NET 10 + Angular production build
2. **Tests** → xUnit (backend) + résultats publiés dans DevOps
3. **Deploy** → Azure App Service (API) + Vercel (Angular)

> 📸 *(Capture d'écran du pipeline à ajouter ici)*
> <img width="1910" height="733" alt="image" src="https://github.com/user-attachments/assets/7fbab593-9892-46d5-b849-a31ac432ac5a" />


---

## 📁 Structure du dépôt
```
AccessiTrack/
├── Backend/
│   ├── AccessiTrack.Domain/          # Entités, interfaces, règles métier
│   ├── AccessiTrack.Application/     # CQRS Commands/Queries, validation
│   ├── AccessiTrack.Infrastructure/  # EF Core, repositories, migrations
│   ├── AccessiTrack.API/             # Contrôleurs REST, Swagger, DI
│   └── AccessiTrack.Tests/           # Tests unitaires xUnit
├── Frontend/
│   └── accessitrack-client/          # Application Angular 18
├── Docs/
│   └── architecture-diagram.drawio   # Diagramme d'architecture
└── azure-pipelines.yml               # Pipeline CI/CD
```

---

## 👤 Auteur

**Charron Philippe**  
Développeur .NET & Spécialiste Accessibilité Web  
Ancien stagiaire — CRTC (Conseil de la radiodiffusion et des  
télécommunications canadiennes)

[![LinkedIn](https://img.shields.io/badge/LinkedIn-profil-blue)](https://linkedin.com/in/philippe-charron-4724582b2/)
[![Azure DevOps](https://img.shields.io/badge/Azure_DevOps-Board-0078D7)](https://dev.azure.com/charron-philippe-dev/AccessiTrack)
