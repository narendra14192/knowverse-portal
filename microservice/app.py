from flask import Flask, jsonify
from flask_cors import CORS

app = Flask(__name__)
# Enable CORS so the main C# backend or the browser client can reach this microservice directly
CORS(app)

# Curated Internship Opportunities Data
internships_data = [
    {
        "id": "microsoft-swe",
        "company": "Microsoft",
        "role": "Software Engineering Intern",
        "eligibility": "Bachelor's/Master's, CS/Engineering/IT",
        "stipend": "Industry Standard",
        "duration": "8–12 weeks",
        "mode": "Full-time, On-site",
        "applyUrl": "https://careers.microsoft.com/v2/global/en/students",
        "tag": "SWE",
        "logoType": "microsoft"
    },
    {
        "id": "tcs-research",
        "company": "TCS",
        "role": "Research / On-Site Internship",
        "eligibility": "Final-year B.Tech, strong academic record",
        "stipend": "₹20,000–₹40,000/month",
        "duration": "12 weeks",
        "mode": "On-site / Hybrid",
        "applyUrl": "https://www.tcs.com/careers/india/internship",
        "tag": "Research",
        "logoType": "tcs"
    },
    {
        "id": "amazon-sde",
        "company": "Amazon",
        "role": "Software Development Engineer Intern",
        "eligibility": "Bachelor's+ in CS/STEM, coding fundamentals",
        "stipend": "₹80,000–₹1.2L/month equivalent",
        "duration": "12 weeks (Fall 2026)",
        "mode": "On-site (US)",
        "applyUrl": "https://www.amazon.jobs/en/jobs/3116030/software-development-engineer-internship-fall-2026-us",
        "tag": "SWE",
        "logoType": "amazon"
    },
    {
        "id": "deloitte-data",
        "company": "Deloitte",
        "role": "Data Analytics Job Simulation",
        "eligibility": "Open to all students, no experience required",
        "stipend": "Fully Free · Virtual Experience",
        "duration": "Self-paced (Forage, AU)",
        "mode": "Fully Virtual",
        "applyUrl": "https://www.theforage.com/simulations/deloitte-au/data-analytics-s5zy",
        "tag": "FREE",
        "logoType": "deloitte"
    },
    {
        "id": "nova-robotics",
        "company": "Nova Robotics",
        "role": "Aerospace / Mechanical R&D Intern",
        "eligibility": "Aerospace / Mechanical candidates",
        "stipend": "₹10,000–₹50,000/month",
        "duration": "3–6 months",
        "mode": "In-Office",
        "lastDate": "20 Jul 2026",
        "applyUrl": "https://unstop.com/internships/aerospace-mechanical-rd-internship-nova-robotics-private-limited-1717688?lb=krHMyqHr&utm_medium=Share&utm_source=internships&utm_campaign=Ysatyam610",
        "tag": "R&D",
        "logoType": "nova"
    },
    {
        "id": "cloudzapier-security",
        "company": "Cloudzapier",
        "role": "Cyber Security Intern",
        "eligibility": "Cyber Security / CS / IT candidates",
        "stipend": "₹10,000–₹20,000/month",
        "duration": "3 months",
        "mode": "Work From Home",
        "lastDate": "11 Aug 2026",
        "applyUrl": "https://internshala.com/internship/detail/work-from-home-cyber-security-internship-at-cloudzapier1783831728",
        "tag": "Security",
        "logoType": "cloudzapier"
    }
]

# Curated Jobs Data
jobs_data = [
    {
        "company": "Accenture",
        "role": "I&F Decision Sci Practitioner Associate",
        "location": "Navi Mumbai, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01653459_en&title=I%26F+Decision+Sci+Practitioner+Associate"
    },
    {
        "company": "Accenture",
        "role": "Analytics and Modeling Associate",
        "location": "Gurugram, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652892_en&title=Analytics+and+Modeling+Associate"
    },
    {
        "company": "Accenture",
        "role": "Analytics and Modeling Associate",
        "location": "Gurugram, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652283_en&title=Analytics+and+Modeling+Associate"
    },
    {
        "company": "Accenture",
        "role": "Analytics and Modeling Associate",
        "location": "Gurugram, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01651763_en&title=Analytics+and+Modeling+Associate"
    },
    {
        "company": "Accenture",
        "role": "Infra Tech Support Practitioner",
        "location": "Indore, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5288362-S1948498_en&title=Infra+Tech+Support+Practitioner"
    },
    {
        "company": "Accenture",
        "role": "Technology Support Engineer",
        "location": "Indore, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5303823-S1970443_en&title=Technology+Support+Engineer"
    },
    {
        "company": "Accenture",
        "role": "Application Support Engineer",
        "location": "Hyderabad, IN",
        "applyUrl": "https://www.accenture.com/in-en/careers/jobdetails?id=14422158_en&title=Application+Support+Engineer"
    }
]

@app.route('/api/scraped-opportunities', methods=['GET'])
def get_opportunities():
    return jsonify(internships_data)

@app.route('/api/scraped-jobs', methods=['GET'])
def get_jobs():
    return jsonify(jobs_data)

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "healthy", "service": "Flask Scraper Microservice"})

if __name__ == '__main__':
    # Listen on localhost:5000
    app.run(host='127.0.0.1', port=5000, debug=True)
