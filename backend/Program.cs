using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Hardcoded Fallbacks in case the Flask microservice is down
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
        client.Timeout = TimeSpan.FromSeconds(3); // Fast timeout in case Flask microservice is off
        var response = await client.GetAsync("http://127.0.0.1:5000/api/scraped-opportunities");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<object>(content);
            return Results.Ok(json);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Flask microservice unreachable: {ex.Message}. Using C# fallback data.");
    }
    return Results.Ok(fallbackOpportunities);
});

app.MapGet("/api/jobs", async (IHttpClientFactory clientFactory) =>
{
    try
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);
        var response = await client.GetAsync("http://127.0.0.1:5000/api/scraped-jobs");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<object>(content);
            return Results.Ok(json);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[C# Gateway] Flask microservice unreachable: {ex.Message}. Using C# fallback data.");
    }
    return Results.Ok(fallbackJobs);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", gateway = "ASP.NET Core API Gateway" }));

// Listen on localhost:5200
app.Run("http://127.0.0.1:5200");
