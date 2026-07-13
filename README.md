# Knowverse Portal: Dynamic Opportunities Hub & Mini-Games

Welcome to the **Knowverse Portal**! This is a state-of-the-art, feature-rich student dashboard built with a resilient, high-availability architecture. It connects early-career job search opportunities, student ambassador programs, live internship listings, and stress-relieving mini-games in a unified, space-dark user interface.

---

## 🌟 Key Features

### 1. High-Availability Backend Gateway (C#)
- **API Gateway (C# / ASP.NET Core Web API)**: Serves as the primary entry point for the client application. It manages routing, processes cross-origin policies (CORS), and connects to The Muse REST API.
- **Fail-Safe Client-Side Pipeline**: If the local C# API Gateway is blocked or offline (e.g. because of Windows Defender Application Control policies), the frontend client automatically fails over to query The Muse REST API directly using client-side async fetch routines.
- **Resilient Fallback Database**: If all network fetches fail, the portal loads curated high-availability cache records (Microsoft, TCS, Amazon, Deloitte, Nova Robotics, Cloudzapier, and Accenture) locally.

### 2. Premium Space-Dark Design ("Aura Dusk")
- Built with a sleek HSL color scheme featuring custom neon borders, glowing backdrop filters (`glassmorphic`), and animated background ambient orbs that follow subtle translation paths.
- Responsive grids and card components that scale down dynamically to mobile viewports.

### 3. Integrated Music Player
- Custom background music controller pinned to the navigation bar.
- Continuous sequential playback of CC0/Royalty-free modern audio tracks.
- Autoplay browser policy check: automatically displays a dark overlay explaining *"Click anywhere to enable background music"* if blocked by browser configurations.
- Hover-expanded volume controls.

### 4. Play & Win: Sudoku Game
- Fully integrated 9x9 interactive Sudoku board with automated timing loops and visual completion feedback.

---

## 🛠️ Technology Stack
- **Frontend**: HTML5, Vanilla CSS3 (Custom Grid/Flexbox), JavaScript (ES6+), Lucide Icons
- **Backend API**: .NET 10.0 (ASP.NET Core Web API)
- **REST Integrations**: The Muse Developer API v2

---

## 🚀 Getting Started

### Prerequisites
- **Git** installed on your local environment.
- **.NET 10 SDK** (for C# API).

### Installation & Run

#### 1. Clone the Repository
```bash
git clone <your-repository-url>
cd "insta link page"
```

#### 2. Run the C# API Gateway
```bash
cd backend
dotnet run
```
*The C# API Gateway will start listening on `http://127.0.0.1:5200`.*

#### 3. Load the Web App
Open `index.html` directly in your browser, or host it using a static file server (e.g. Live Server extension in VS Code, or using Node.js) on port `8080`.

---

## 📐 API Endpoint Specifications

### C# API Gateway (Port 5200)
- `GET /api/opportunities`: Fetches and filters internships from The Muse REST API.
- `GET /api/jobs`: Fetches entry-level careers from The Muse REST API.
- `GET /health`: Basic health state monitoring.
