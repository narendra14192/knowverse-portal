# Knowverse Portal: Dynamic Opportunities Hub & Mini-Games

Welcome to the **Knowverse Portal**! This is a state-of-the-art, feature-rich student dashboard built with a resilient, high-availability architecture. It connects early-career job search opportunities, student ambassador programs, live internship listings, and stress-relieving mini-games in a unified, space-dark user interface.

---

## 🌟 Key Features

### 1. High-Availability Multi-API Backend Gateway (C#)
- **Unified API Gateway (C# / ASP.NET Core Web API)**: Connects and aggregates results dynamically from multiple upstream job search systems:
  - **Adzuna API**: Integrated with full country code queries and paginated routing.
  - **JSearch (RapidAPI)**: Integrated for high-relevancy global developer and technical search queries.
  - **Remotive API**: Custom category search for remote-first roles.
  - **The Muse API**: Full-scale fallback system.
- **Fail-Safe Client-Side Pipeline**: If the local C# API Gateway is blocked or offline, the frontend client automatically fails over to query the live REST APIs directly using client-side async fetch routines with identical parameter structures.
- **Resilient Fallback Database**: If all network fetches fail, the portal loads curated high-availability cache records (Microsoft, TCS, Amazon, Deloitte, Nova Robotics, Cloudzapier, and Accenture) locally.

### 2. Live Search, Custom Category Filters & Location Mapping
- Custom glassmorphic search input and filters with interactive category pills (SWE, Data, Design, Security, and general Internships).
- Live country selectors supporting localized regional routing to downstream API engines.
- Built-in client-side input debounce (300ms) to control API request frequencies and avoid rate limits.

### 3. Load More / Infinite Pagination
- Implemented stateful pagination (both backend endpoint propagation and frontend query loops).
- Premium, animated glassmorphic "Load More" controls for both Internships and Jobs, appending listings seamlessly without reloading.

### 4. Premium Space-Dark Design ("Aura Dusk")
- Built with a sleek HSL color scheme featuring custom neon borders, glowing backdrop filters (`glassmorphic`), and animated background ambient orbs.
- Responsive grids and card components that scale down dynamically to mobile viewports.

### 5. Integrated Music Player
- Custom background music controller pinned to the navigation bar.
- Continuous sequential playback of CC0/Royalty-free modern audio tracks.
- Autoplay browser policy check: automatically displays a dark overlay explaining *"Click anywhere to enable background music"* if blocked by browser configurations.
- Hover-expanded volume controls.

### 6. Play & Win: Sudoku Game
- Fully integrated 9x9 interactive Sudoku board with automated timing loops and visual completion feedback.

---

## 🛠️ Technology Stack
- **Frontend**: HTML5, Vanilla CSS3 (Custom Grid/Flexbox), JavaScript (ES6+), Lucide Icons
- **Backend API**: .NET 8.0 (ASP.NET Core Web API)
- **REST Integrations**: Adzuna API, JSearch API (RapidAPI), Remotive API, The Muse Developer API v2

---

## 🚀 Getting Started

### Prerequisites
- **Git** installed on your local environment.
- **.NET 8 SDK** (for C# API).

### Installation & Run

#### 1. Clone the Repository
```bash
git clone <your-repository-url>
git checkout main
```

#### 2. Run the C# API Gateway
```bash
cd backend
dotnet run
```
*The C# API Gateway will start listening on `http://127.0.0.1:5200`.*

#### 3. Load the Web App
Open `index.html` directly in your browser, or host the `frontend/` directory using a static file server (e.g. VS Code Live Server, or local http-server) on port `8080`.

---

## 📐 API Endpoint Specifications

### C# API Gateway (Port 5200)
- `GET /api/opportunities?q={query}&country={countryCode}&page={page}`: Fetches and filters internships from the upstream APIs.
- `GET /api/jobs?q={query}&country={countryCode}&page={page}`: Fetches entry-level careers from the upstream APIs.
- `GET /api/certifications?category={category}&q={query}&page={page}`: Serves curated certifications and free skill credentials (Google Cloud, AWS, Microsoft, Meta, IBM, Harvard CS50, freeCodeCamp, Forage, Postman).
- `GET /health`: Basic health state monitoring.

