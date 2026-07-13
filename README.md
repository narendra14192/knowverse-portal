# Knowverse Portal: Dynamic Opportunities Hub & Mini-Games

Welcome to the **Knowverse Portal**! This is a state-of-the-art, feature-rich student dashboard built with a dual-backend architecture. It connects early-career job search opportunities, student ambassador programs, live internship listings, and stress-relieving mini-games in a unified, space-dark user interface.

---

## 🌟 Key Features

### 1. Dual-Backend Architecture
- **API Gateway (C# / ASP.NET Core Web API)**: Serves as the primary entry point for the client application. It manages routing, processes cross-origin policies (CORS), and acts as a high-availability controller.
- **Microservice (Python / Flask)**: A dedicated scraper and data collector microservice that compiles internship opportunities from portals.
- **Fail-Safe High Availability**: If the Flask microservice is offline, the C# Gateway automatically falls back to an integrated, built-in memory cache database, guaranteeing zero downtime.

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
- **Primary Backend API**: .NET 10.0 (ASP.NET Core Web API)
- **Scraper Microservice**: Python 3.9+ / Flask / Flask-CORS

---

## 🚀 Getting Started

### Prerequisites
- **Git** installed on your local environment.
- **.NET 10 SDK** (for C# API).
- **Python 3.9+** (for Flask scraper).

### Installation & Run

#### 1. Clone the Repository
```bash
git clone <your-repository-url>
cd "insta link page"
```

#### 2. Run the Python Flask Microservice
```bash
cd microservice
pip install -r requirements.txt
python app.py
```
*The Flask microservice will start listening on `http://127.0.0.1:5000`.*

#### 3. Run the C# API Gateway
In a new terminal window:
```bash
cd backend
dotnet run
```
*The C# API Gateway will start listening on `http://127.0.0.1:5200`.*

#### 4. Load the Web App
Open `index.html` directly in your browser, or host it using a static file server (e.g. Live Server extension in VS Code, or using Node.js) on port `8080`.

---

## 📐 API Endpoint Specifications

### C# API Gateway (Port 5200)
- `GET /api/opportunities`: Calls Flask or returns high-availability C# fallbacks.
- `GET /api/jobs`: Returns live Accenture careers compiled by the Flask scraper.
- `GET /health`: Basic health state monitoring.

### Python Flask Service (Port 5000)
- `GET /api/scraped-opportunities`: Returns dynamic compiled list of internships.
- `GET /api/scraped-jobs`: Returns scraped Accenture vacancies.
