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
var rapidApiKey = config["ApiSettings:RapidApiKey"] ?? "7c9340a0b7mshd4f471766795179p161947jsnda2a0b78f8c1";
var rapidApiHost = config["ApiSettings:RapidApiHost"] ?? "jsearch.p.rapidapi.com";
var adzunaAppId = config["ApiSettings:AdzunaAppId"] ?? "ad843124";
var adzunaAppKey = config["ApiSettings:AdzunaAppKey"] ?? "84173c51c3d2cc118bca43dc9df0ad1c";

// Resolve frontend folder path (works locally, on Render, and in Docker)
var frontendPath = config["ApiSettings:FrontendPath"];
if (string.IsNullOrEmpty(frontendPath))
{
    // Auto-detect: try paths in order of likelihood
    var dockerPath    = Path.GetFullPath("./frontend");
    var repoRoot      = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    var repoFrontend  = Path.Combine(repoRoot, "frontend");
    var localPath     = Path.GetFullPath("../frontend");

    if (Directory.Exists(dockerPath))      frontendPath = dockerPath;
    else if (Directory.Exists(repoFrontend)) frontendPath = repoFrontend;
    else if (Directory.Exists(localPath))  frontendPath = localPath;
    else                                   frontendPath = dockerPath;
}
else
{
    // Always convert to absolute — PhysicalFileProvider requires absolute paths
    frontendPath = Path.GetFullPath(frontendPath);
}

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

var fallbackCertifications = new[]
{
    new {
        id = "gcp-cloud-digital-leader",
        title = "Google Cloud Digital Leader & Skill Badges",
        provider = "Google Cloud",
        category = "cloud",
        isFree = true,
        skills = new[] { "Cloud Computing", "GCP", "Infrastructure", "Security" },
        duration = "Self-paced (10–15 hrs)",
        level = "Beginner",
        applyUrl = "https://www.cloudskillsboost.google/",
        tag = "FREE BADGE",
        logoType = "google"
    },
    new {
        id = "aws-educate-cloud",
        title = "AWS Educate Cloud Practitioner & Badges",
        provider = "Amazon Web Services",
        category = "cloud",
        isFree = true,
        skills = new[] { "AWS Core Services", "Cloud Architecture", "S3", "EC2" },
        duration = "Self-paced (20 hrs)",
        level = "Beginner to Intermediate",
        applyUrl = "https://aws.amazon.com/education/awseducate/",
        tag = "100% FREE",
        logoType = "aws"
    },
    new {
        id = "ms-azure-fundamentals",
        title = "Microsoft Azure Fundamentals (AZ-900) & Applied Skills",
        provider = "Microsoft Learn",
        category = "cloud",
        isFree = true,
        skills = new[] { "Azure Cloud", "Identity", "Governance", "Virtual Machines" },
        duration = "Self-paced",
        level = "Beginner",
        applyUrl = "https://learn.microsoft.com/en-us/credentials/",
        tag = "FREE LEARNING",
        logoType = "microsoft"
    },
    new {
        id = "meta-frontend-dev",
        title = "Meta Front-End Developer Professional Certificate",
        provider = "Meta (Coursera)",
        category = "software",
        isFree = false,
        skills = new[] { "React.js", "JavaScript", "HTML5/CSS3", "UX/UI Principles" },
        duration = "6 months (5 hrs/wk)",
        level = "Beginner",
        applyUrl = "https://www.coursera.org/professional-certificates/meta-front-end-developer",
        tag = "VERIFIED CERTIFICATE",
        logoType = "meta"
    },
    new {
        id = "harvard-cs50",
        title = "CS50: Introduction to Computer Science",
        provider = "Harvard University (edX)",
        category = "software",
        isFree = true,
        skills = new[] { "C", "Python", "SQL", "Algorithms", "Data Structures" },
        duration = "12 weeks (6–18 hrs/wk)",
        level = "Beginner",
        applyUrl = "https://pll.harvard.edu/course/cs50-introduction-computer-science",
        tag = "100% FREE AUDIT",
        logoType = "harvard"
    },
    new {
        id = "freecodecamp-web",
        title = "Responsive Web Design & JavaScript Algorithms",
        provider = "freeCodeCamp",
        category = "software",
        isFree = true,
        skills = new[] { "HTML5", "CSS3", "ES6+ JavaScript", "Algorithms" },
        duration = "300 hours (Self-paced)",
        level = "Beginner",
        applyUrl = "https://www.freecodecamp.org/",
        tag = "FREE CERTIFICATION",
        logoType = "freecodecamp"
    },
    new {
        id = "ibm-data-science",
        title = "IBM Data Science & AI Engineering Professional Certificate",
        provider = "IBM SkillsBuild",
        category = "ai",
        isFree = true,
        skills = new[] { "Python", "Data Analysis", "Machine Learning", "SQL", "Pandas" },
        duration = "Self-paced",
        level = "Intermediate",
        applyUrl = "https://skillsbuild.org/",
        tag = "FREE FOR STUDENTS",
        logoType = "ibm"
    },
    new {
        id = "cisco-cybersecurity",
        title = "Cybersecurity Essentials & Network Defense",
        provider = "Cisco Networking Academy",
        category = "cyber",
        isFree = true,
        skills = new[] { "Network Security", "Threat Intelligence", "Ethical Hacking" },
        duration = "30 hours",
        level = "Intermediate",
        applyUrl = "https://www.netacad.com/",
        tag = "100% FREE",
        logoType = "cisco"
    },
    new {
        id = "forage-tech-simulations",
        title = "Deloitte & Accenture Tech Virtual Job Simulations",
        provider = "Forage",
        category = "software",
        isFree = true,
        skills = new[] { "Data Analytics", "Software Engineering", "Cyber Defense" },
        duration = "5–6 hours (Self-paced)",
        level = "All Levels",
        applyUrl = "https://www.theforage.com/",
        tag = "FREE CERTIFICATE",
        logoType = "forage"
    },
    new {
        id = "postman-api-expert",
        title = "Postman API Fundamentals Student Expert",
        provider = "Postman",
        category = "software",
        isFree = true,
        skills = new[] { "REST APIs", "Postman Collections", "HTTP Methods", "API Testing" },
        duration = "3 hours",
        level = "Beginner",
        applyUrl = "https://studentexperts.postman.com/",
        tag = "BADGE & CERTIFICATE",
        logoType = "postman"
    },
    new {
        id = "infosys-springboard",
        title = "Web Development, Python & Full-Stack Courses",
        provider = "Infosys Springboard",
        category = "software",
        isFree = true,
        skills = new[] { "Python", "HTML", "CSS", "JavaScript", "SQL", "Web Dev" },
        duration = "Self-paced",
        level = "Beginner to Intermediate",
        applyUrl = "https://infyspringboard.onwingspan.com",
        tag = "FREE CERTIFICATE",
        logoType = "infosys"
    },
    new {
        id = "great-learning-academy",
        title = "Python, Django & Software Engineering Courses",
        provider = "Great Learning Academy",
        category = "software",
        isFree = true,
        skills = new[] { "Python", "Django", "Web Development", "Data Science" },
        duration = "Self-paced (2–4 hrs)",
        level = "Beginner",
        applyUrl = "https://www.mygreatlearning.com/academy",
        tag = "FREE CERTIFICATE",
        logoType = "greatlearning"
    },
    new {
        id = "simplilearn-skillup",
        title = "Free Python & Programming SkillUp Courses",
        provider = "Simplilearn SkillUp",
        category = "software",
        isFree = true,
        skills = new[] { "Python", "Data Science", "Coding Fundamentals" },
        duration = "Self-paced (4–9 hrs)",
        level = "Beginner",
        applyUrl = "https://www.skillup.simplilearn.com",
        tag = "FREE CERTIFICATE",
        logoType = "simplilearn"
    },
    new {
        id = "kaggle-learn-python",
        title = "Kaggle Learn: Python & Data Science Micro-Courses",
        provider = "Kaggle",
        category = "ai",
        isFree = true,
        skills = new[] { "Python", "Data Analysis", "Pandas", "Machine Learning" },
        duration = "Self-paced (5 hrs)",
        level = "Beginner",
        applyUrl = "https://www.kaggle.com/learn",
        tag = "FREE BADGE & CERTIFICATE",
        logoType = "kaggle"
    },
    new {
        id = "cisco-python-essentials",
        title = "Cisco Python Essentials (Skills for All)",
        provider = "Cisco Networking Academy",
        category = "software",
        isFree = true,
        skills = new[] { "Python", "Object-Oriented Programming", "Algorithms" },
        duration = "75 hours (Self-paced)",
        level = "Beginner to Intermediate",
        applyUrl = "https://www.netacad.com/courses/python",
        tag = "FREE CERTIFICATE",
        logoType = "cisco"
    }
};

// Endpoints
app.MapGet("/api/certifications", (string? q, string? category, int? page) =>
{
    var list = fallbackCertifications.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(category) && category.ToLower() != "all")
    {
        string catLower = category.ToLower();
        if (catLower == "free")
        {
            list = list.Where(c => c.isFree);
        }
        else
        {
            list = list.Where(c => c.category.Equals(catLower, StringComparison.OrdinalIgnoreCase));
        }
    }

    if (!string.IsNullOrWhiteSpace(q))
    {
        string queryLower = q.ToLower();
        list = list.Where(c => 
            c.title.ToLower().Contains(queryLower) ||
            c.provider.ToLower().Contains(queryLower) ||
            c.skills.Any(s => s.ToLower().Contains(queryLower)) ||
            c.tag.ToLower().Contains(queryLower)
        );
    }

    int pageNum = page ?? 1;
    int pageSize = 6;
    var pagedList = list.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList();

    return Results.Ok(pagedList);
});

app.MapGet("/api/opportunities", async (IHttpClientFactory clientFactory, string? q, string? country, int? page) =>

{
    string query = string.IsNullOrEmpty(q) ? "internship" : q;
    string locationCode = string.IsNullOrEmpty(country) ? "in" : country.ToLower();
    int pageNum = page ?? 1;

    // Try Adzuna API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var url = $"https://api.adzuna.com/v1/api/jobs/{locationCode}/search/{pageNum}?app_id={adzunaAppId}&app_key={adzunaAppKey}&what={Uri.EscapeDataString(query)}&content-type=application/json";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var content = System.Text.Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                int count = 0;
                foreach (var item in resultsElement.EnumerateArray())
                {
                    if (count >= 6) break;
                    
                    string id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
                    
                    string company = "Unknown Company";
                    if (item.TryGetProperty("company", out var compObj) && compObj.TryGetProperty("display_name", out var compNameProp))
                    {
                        company = compNameProp.GetString() ?? "Unknown Company";
                    }
                    
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "Internship Vacancy" : "Internship Vacancy";
                    
                    string location = "Remote / India";
                    if (item.TryGetProperty("location", out var locObj) && locObj.TryGetProperty("display_name", out var locNameProp))
                    {
                        location = locNameProp.GetString() ?? "Remote / India";
                    }
                    
                    string tag = "Intern";
                    string roleLower = role.ToLower();
                    if (roleLower.Contains("software") || roleLower.Contains("engineering") || roleLower.Contains("developer")) tag = "SWE";
                    else if (roleLower.Contains("data") || roleLower.Contains("analytics")) tag = "Data";
                    else if (roleLower.Contains("design") || roleLower.Contains("creative")) tag = "Design";
                    else if (roleLower.Contains("security") || roleLower.Contains("cyber")) tag = "Security";
                    
                    string applyUrl = item.TryGetProperty("redirect_url", out var urlProp) ? urlProp.GetString() ?? "https://www.adzuna.in" : "https://www.adzuna.in";
                    
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
                    Console.WriteLine("[C# Gateway] Loaded opportunities from Adzuna API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Adzuna API failed: {ex.Message}. Trying JSearch API...");
    }

    // Try JSearch (RapidAPI)
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://{rapidApiHost}/search?query={Uri.EscapeDataString(query)}&location={locationCode}&page={pageNum}&num_pages=1"),
            Headers =
            {
                { "x-rapidapi-key", rapidApiKey },
                { "x-rapidapi-host", rapidApiHost }
            }
        };
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                int count = 0;
                foreach (var item in dataElement.EnumerateArray())
                {
                    if (count >= 6) break;
                    
                    string id = item.TryGetProperty("job_id", out var idProp) ? idProp.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
                    string company = item.TryGetProperty("employer_name", out var compProp) ? compProp.GetString() ?? "Unknown Company" : "Unknown Company";
                    string role = item.TryGetProperty("job_title", out var roleProp) ? roleProp.GetString() ?? "Internship Vacancy" : "Internship Vacancy";
                    
                    string? city = item.TryGetProperty("job_city", out var cityProp) ? cityProp.GetString() : null;
                    string? countryName = item.TryGetProperty("job_country", out var countryProp) ? countryProp.GetString() : null;
                    string location = (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(countryName)) ? $"{city}, {countryName}" : "Remote / USA";
                    
                    string tag = "Intern";
                    string roleLower = role.ToLower();
                    if (roleLower.Contains("software") || roleLower.Contains("engineering") || roleLower.Contains("developer")) tag = "SWE";
                    else if (roleLower.Contains("data") || roleLower.Contains("analytics")) tag = "Data";
                    else if (roleLower.Contains("design") || roleLower.Contains("creative")) tag = "Design";
                    else if (roleLower.Contains("security") || roleLower.Contains("cyber")) tag = "Security";
                    
                    string applyUrl = item.TryGetProperty("job_apply_link", out var applyProp) ? applyProp.GetString() ?? "https://jsearch.p.rapidapi.com" : "https://jsearch.p.rapidapi.com";
                    
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
                    Console.WriteLine("[C# Gateway] Loaded opportunities from JSearch API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] JSearch API failed: {ex.Message}. Trying Remotive API...");
    }

    // Try Remotive API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://remotive.com/api/remote-jobs?category=software-development");
        request.Headers.Add("User-Agent", "Knowverse-Portal/1.0");
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("jobs", out var jobsElement) && jobsElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                var itemsArray = jobsElement.EnumerateArray().ToList();
                var filteredItems = new List<JsonElement>();
                foreach (var item in itemsArray)
                {
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
                    if (query == "internship")
                    {
                        if (!role.ToLower().Contains("intern")) continue;
                    }
                    else
                    {
                        if (!role.ToLower().Contains(query.ToLower())) continue;
                    }
                    filteredItems.Add(item);
                }

                int itemsToSkip = (pageNum - 1) * 6;
                var pagedItems = filteredItems.Skip(itemsToSkip).Take(6);
                int count = 0;
                foreach (var item in pagedItems)
                {
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
                    string id = item.TryGetProperty("id", out var idProp) ? idProp.GetInt64().ToString() : Guid.NewGuid().ToString();
                    string company = item.TryGetProperty("company_name", out var compProp) ? compProp.GetString() ?? "Unknown Company" : "Unknown Company";
                    string location = item.TryGetProperty("candidate_required_location", out var locProp) ? locProp.GetString() ?? "Remote" : "Remote";
                    
                    string tag = "Intern";
                    string roleLower = role.ToLower();
                    if (roleLower.Contains("software") || roleLower.Contains("engineering") || roleLower.Contains("developer")) tag = "SWE";
                    else if (roleLower.Contains("data") || roleLower.Contains("analytics")) tag = "Data";
                    else if (roleLower.Contains("design") || roleLower.Contains("creative")) tag = "Design";
                    else if (roleLower.Contains("security") || roleLower.Contains("cyber")) tag = "Security";
                    
                    string applyUrl = item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "https://remotive.com" : "https://remotive.com";
                    
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
                    Console.WriteLine("[C# Gateway] Loaded opportunities from Remotive API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Remotive API failed: {ex.Message}. Trying Muse API...");
    }

    // Fallback to Muse API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4); // 4-second timeout limit
        
        var url = $"https://www.themuse.com/api/public/jobs?level=Internship&page={pageNum - 1}&api_key={ApiKey}";
        if (query != "internship")
        {
            url += $"&desc={Uri.EscapeDataString(query)}";
        }
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

app.MapGet("/api/jobs", async (IHttpClientFactory clientFactory, string? q, string? country, int? page) =>
{
    string query = string.IsNullOrEmpty(q) ? "software developer entry level" : q;
    string locationCode = string.IsNullOrEmpty(country) ? "in" : country.ToLower();
    int pageNum = page ?? 1;

    // Try Adzuna API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var url = $"https://api.adzuna.com/v1/api/jobs/{locationCode}/search/{pageNum}?app_id={adzunaAppId}&app_key={adzunaAppKey}&what={Uri.EscapeDataString(query)}&content-type=application/json";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var content = System.Text.Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                int count = 0;
                foreach (var item in resultsElement.EnumerateArray())
                {
                    if (count >= 7) break; // Keep layout table exactly 7 items
                    
                    string company = "Unknown Company";
                    if (item.TryGetProperty("company", out var compObj) && compObj.TryGetProperty("display_name", out var compNameProp))
                    {
                        company = compNameProp.GetString() ?? "Unknown Company";
                    }
                    
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "Job Role" : "Job Role";
                    
                    string location = "Remote / India";
                    if (item.TryGetProperty("location", out var locObj) && locObj.TryGetProperty("display_name", out var locNameProp))
                    {
                        location = locNameProp.GetString() ?? "Remote / India";
                    }
                    
                    string applyUrl = item.TryGetProperty("redirect_url", out var urlProp) ? urlProp.GetString() ?? "https://www.adzuna.in" : "https://www.adzuna.in";

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
                    Console.WriteLine("[C# Gateway] Loaded jobs from Adzuna API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Adzuna API failed: {ex.Message}. Trying JSearch API...");
    }

    // Try JSearch (RapidAPI)
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://{rapidApiHost}/search?query={Uri.EscapeDataString(query)}&location={locationCode}&page={pageNum}&num_pages=1"),
            Headers =
            {
                { "x-rapidapi-key", rapidApiKey },
                { "x-rapidapi-host", rapidApiHost }
            }
        };
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                int count = 0;
                foreach (var item in dataElement.EnumerateArray())
                {
                    if (count >= 7) break; // Keep layout table exactly 7 items
                    
                    string company = item.TryGetProperty("employer_name", out var compProp) ? compProp.GetString() ?? "Unknown Company" : "Unknown Company";
                    string role = item.TryGetProperty("job_title", out var roleProp) ? roleProp.GetString() ?? "Job Role" : "Job Role";
                    
                    string? city = item.TryGetProperty("job_city", out var cityProp) ? cityProp.GetString() : null;
                    string? countryName = item.TryGetProperty("job_country", out var countryProp) ? countryProp.GetString() : null;
                    string location = (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(countryName)) ? $"{city}, {countryName}" : "Various Locations";
                    
                    string applyUrl = item.TryGetProperty("job_apply_link", out var applyProp) ? applyProp.GetString() ?? "https://jsearch.p.rapidapi.com" : "https://jsearch.p.rapidapi.com";

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
                    Console.WriteLine("[C# Gateway] Loaded jobs from JSearch API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] JSearch API failed: {ex.Message}. Trying Remotive API...");
    }

    // Try Remotive API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://remotive.com/api/remote-jobs?category=software-development");
        request.Headers.Add("User-Agent", "Knowverse-Portal/1.0");
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("jobs", out var jobsElement) && jobsElement.ValueKind == JsonValueKind.Array)
            {
                var parsedList = new List<object>();
                var itemsArray = jobsElement.EnumerateArray().ToList();
                var filteredItems = new List<JsonElement>();
                foreach (var item in itemsArray)
                {
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
                    if (query != "software developer entry level" && !role.ToLower().Contains(query.ToLower())) continue;
                    filteredItems.Add(item);
                }

                int itemsToSkip = (pageNum - 1) * 7;
                var pagedItems = filteredItems.Skip(itemsToSkip).Take(7);
                int count = 0;
                foreach (var item in pagedItems)
                {
                    string company = item.TryGetProperty("company_name", out var compProp) ? compProp.GetString() ?? "Unknown Company" : "Unknown Company";
                    string role = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "Job Role" : "Job Role";
                    string location = item.TryGetProperty("candidate_required_location", out var locProp) ? locProp.GetString() ?? "Remote" : "Remote";
                    string applyUrl = item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "https://remotive.com" : "https://remotive.com";

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
                    Console.WriteLine("[C# Gateway] Loaded jobs from Remotive API.");
                    return Results.Ok(parsedList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Remotive API failed: {ex.Message}. Trying Muse API...");
    }

    // Fallback to Muse API
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(4);
        
        var url = $"https://www.themuse.com/api/public/jobs?level=Entry%20Level&page={pageNum - 1}&api_key={ApiKey}";
        if (query != "software developer entry level")
        {
            url += $"&desc={Uri.EscapeDataString(query)}";
        }
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

// Read PORT from environment (Render sets this automatically)
// Locally defaults to 5200
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
app.Run($"http://0.0.0.0:{port}");
