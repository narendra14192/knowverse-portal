using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.json
var config = builder.Configuration;
var museApiKey = config["ApiSettings:MuseApiKey"] ?? "e267525eb7f40f0bdee9a81184697d495998da702a5742cc233667e688e74212";

// Resolve frontend folder path (sibling directory to backend)
var frontendPath = config["ApiSettings:FrontendPath"] 
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "frontend"));

// Add services
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    // Allow same-origin requests from the backend itself (serving static files)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// ─── Serve Frontend Static Files ───────────────────────────────────────────
// The ASP.NET Core backend hosts the frontend directly — no Node.js needed.
if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    Console.WriteLine($"[Knowverse] Serving frontend from: {frontendPath}");
}
else
{
    Console.WriteLine($"[Knowverse] WARNING: Frontend folder not found at {frontendPath}");
}
// ────────────────────────────────────────────────────────────────────────────

// API key from configuration
string ApiKey = museApiKey;

// Hardcoded Fallbacks in case The Muse API is down or rate-limited
var fallbackOpportunities = new[]
{
    new {
        id = "microsoft-swe",
        company = "Microsoft",
        role = "Software Engineering Intern",
        eligibility = "Bachelor's/Master's, CS/Engineering/IT",
        stipend = "Industry Standard",
        duration = "8–12 weeks",
        mode = "Full-time, On-site",
        applyUrl = "https://careers.microsoft.com/v2/global/en/students",
        tag = "SWE",
        logoType = "microsoft",
        lastDate = (string?)null
    },
    new {
        id = "tcs-research",
        company = "TCS",
        role = "Research / On-Site Internship",
        eligibility = "Final-year B.Tech, strong academic record",
        stipend = "₹20,000–₹40,000/month",
        duration = "12 weeks",
        mode = "On-site / Hybrid",
        applyUrl = "https://www.tcs.com/careers/india/internship",
        tag = "Research",
        logoType = "tcs",
        lastDate = (string?)null
    },
    new {
        id = "amazon-sde",
        company = "Amazon",
        role = "Software Development Engineer Intern",
        eligibility = "Bachelor's+ in CS/STEM, coding fundamentals",
        stipend = "₹80,000–₹1.2L/month equivalent",
        duration = "12 weeks (Fall 2026)",
        mode = "On-site (US)",
        applyUrl = "https://www.amazon.jobs/en/jobs/3116030/software-development-engineer-internship-fall-2026-us",
        tag = "SWE",
        logoType = "amazon",
        lastDate = (string?)null
    },
    new {
        id = "deloitte-data",
        company = "Deloitte",
        role = "Data Analytics Job Simulation",
        eligibility = "Open to all students, no experience required",
        stipend = "Fully Free · Virtual Experience",
        duration = "Self-paced (Forage, AU)",
        mode = "Fully Virtual",
        applyUrl = "https://www.theforage.com/simulations/deloitte-au/data-analytics-s5zy",
        tag = "FREE",
        logoType = "deloitte",
        lastDate = (string?)null
    },
    new {
        id = "nova-robotics",
        company = "Nova Robotics",
        role = "Aerospace / Mechanical R&D Intern",
        eligibility = "Aerospace / Mechanical candidates",
        stipend = "₹10,000–₹50,000/month",
        duration = "3–6 months",
        mode = "In-Office",
        applyUrl = "https://unstop.com/internships/aerospace-mechanical-rd-internship-nova-robotics-private-limited-1717688?lb=krHMyqHr&utm_medium=Share&utm_source=internships&utm_campaign=Ysatyam610",
        tag = "R&D",
        logoType = "nova",
        lastDate = (string?)"20 Jul 2026"
    },
    new {
        id = "cloudzapier-security",
        company = "Cloudzapier",
        role = "Cyber Security Intern",
        eligibility = "Cyber Security / CS / IT candidates",
        stipend = "₹10,000–₹20,000/month",
        duration = "3 months",
        mode = "Work From Home",
        applyUrl = "https://internshala.com/internship/detail/work-from-home-cyber-security-internship-at-cloudzapier1783831728",
        tag = "Security",
        logoType = "cloudzapier",
        lastDate = (string?)"11 Aug 2026"
    }
};

var fallbackJobs = new[]
{
    new {
        company = "Accenture",
        role = "I&F Decision Sci Practitioner Associate",
        location = "Navi Mumbai, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01653459_en&title=I%26F+Decision+Sci+Practitioner+Associate"
    },
    new {
        company = "Accenture",
        role = "Analytics and Modeling Associate",
        location = "Gurugram, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652892_en&title=Analytics+and+Modeling+Associate"
    },
    new {
        company = "Accenture",
        role = "Analytics and Modeling Associate",
        location = "Gurugram, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652283_en&title=Analytics+and+Modeling+Associate"
    },
    new {
        company = "Accenture",
        role = "Analytics and Modeling Associate",
        location = "Gurugram, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01651763_en&title=Analytics+and+Modeling+Associate"
    },
    new {
        company = "Accenture",
        role = "Infra Tech Support Practitioner",
        location = "Indore, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5288362-S1948498_en&title=Infra+Tech+Support+Practitioner"
    },
    new {
        company = "Accenture",
        role = "Technology Support Engineer",
        location = "Indore, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5303823-S1970443_en&title=Technology+Support+Engineer"
    },
    new {
        company = "Accenture",
        role = "Application Support Engineer",
        location = "Hyderabad, IN",
        applyUrl = "https://www.accenture.com/in-en/careers/jobdetails?id=14422158_en&title=Application+Support+Engineer"
    }
};

// Endpoints
app.MapGet("/api/opportunities", async (IHttpClientFactory clientFactory) =>
{
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4); // 4-second timeout limit
        
        var url = $"https://www.themuse.com/api/public/jobs?level=Internship&page=0&api_key={ApiKey}";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var resultsElement = doc.RootElement.GetProperty("results");
            
            var parsedList = new List<object>();
            int count = 0;
            foreach (var item in resultsElement.EnumerateArray())
            {
                if (count >= 6) break; // Keep layout grid exactly 6 items
                
                string id = item.GetProperty("id").GetInt32().ToString();
                string company = item.GetProperty("company").GetProperty("name").GetString() ?? "Unknown Company";
                string role = item.GetProperty("name").GetString() ?? "Internship Vacancy";
                
                string location = "Remote / USA";
                if (item.TryGetProperty("locations", out var locs) && locs.ValueKind == JsonValueKind.Array && locs.GetArrayLength() > 0)
                {
                    location = locs[0].GetProperty("name").GetString() ?? "Remote / USA";
                }
                
                string category = "Tech";
                if (item.TryGetProperty("categories", out var cats) && cats.ValueKind == JsonValueKind.Array && cats.GetArrayLength() > 0)
                {
                    category = cats[0].GetProperty("name").GetString() ?? "Tech";
                }
                
                string tag = "Intern";
                if (category.Contains("Software") || category.Contains("Engineering") || category.Contains("Developer")) tag = "SWE";
                else if (category.Contains("Data") || category.Contains("Analytics")) tag = "Data";
                else if (category.Contains("Design") || category.Contains("Creative")) tag = "Design";
                else if (category.Contains("Security") || category.Contains("Cyber")) tag = "Security";
                
                string applyUrl = "https://www.themuse.com";
                if (item.TryGetProperty("refs", out var refs) && refs.TryGetProperty("landing_page", out var lp))
                {
                    applyUrl = lp.GetString() ?? "https://www.themuse.com";
                }
                
                // Unify company logo types
                string logoType = "generic";
                string compLower = company.ToLower();
                if (compLower.Contains("microsoft")) logoType = "microsoft";
                else if (compLower.Contains("amazon")) logoType = "amazon";
                else if (compLower.Contains("deloitte")) logoType = "deloitte";
                else if (compLower.Contains("tcs") || compLower.Contains("tata consultancy")) logoType = "tcs";
                else if (compLower.Contains("nova")) logoType = "nova";
                else if (compLower.Contains("cloudzapier")) logoType = "cloudzapier";

                parsedList.Add(new {
                    id = id,
                    company = company,
                    role = role,
                    eligibility = "Tech / STEM candidates",
                    stipend = "Competitive Stipend",
                    duration = "3–6 months",
                    mode = location,
                    applyUrl = applyUrl,
                    tag = tag,
                    logoType = logoType,
                    lastDate = (string?)null
                });
                count++;
            }

            if (parsedList.Count > 0)
            {
                return Results.Ok(parsedList);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] The Muse API unreachable: {ex.Message}. Falling back to C# static cache database.");
    }
    
    return Results.Ok(fallbackOpportunities);
});

app.MapGet("/api/jobs", async (IHttpClientFactory clientFactory) =>
{
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        
        var url = $"https://www.themuse.com/api/public/jobs?level=Entry%20Level&page=0&api_key={ApiKey}";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var resultsElement = doc.RootElement.GetProperty("results");
            
            var parsedList = new List<object>();
            int count = 0;
            foreach (var item in resultsElement.EnumerateArray())
            {
                if (count >= 7) break; // Keep layout table exactly 7 items
                
                string company = item.GetProperty("company").GetProperty("name").GetString() ?? "Unknown Company";
                string role = item.GetProperty("name").GetString() ?? "Job Role";
                
                string location = "Various Locations";
                if (item.TryGetProperty("locations", out var locs) && locs.ValueKind == JsonValueKind.Array && locs.GetArrayLength() > 0)
                {
                    location = locs[0].GetProperty("name").GetString() ?? "Various Locations";
                }
                
                string applyUrl = "https://www.themuse.com";
                if (item.TryGetProperty("refs", out var refs) && refs.TryGetProperty("landing_page", out var lp))
                {
                    applyUrl = lp.GetString() ?? "https://www.themuse.com";
                }

                parsedList.Add(new {
                    company = company,
                    role = role,
                    location = location,
                    applyUrl = applyUrl
                });
                count++;
            }

            if (parsedList.Count > 0)
            {
                return Results.Ok(parsedList);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] The Muse API unreachable: {ex.Message}. Falling back to C# static cache database.");
    }
    
    return Results.Ok(fallbackJobs);
});

app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    gateway = "ASP.NET Core Pure C# Gateway",
    serves = "Frontend + API (no Node.js, no Python)"
}));

// Single unified server: serves frontend HTML + /api/* endpoints
// Open browser to: http://localhost:5200
app.Run("http://localhost:5200");
