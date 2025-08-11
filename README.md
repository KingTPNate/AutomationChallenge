# 🚀 AutomationChallenge

AutomationChallenge is a C# console app using Microsoft.Playwright to automate updating your GitHub profile bio, including 2FA login handling.

---

## 🔧 Prerequisites

Before running the app, ensure you have:

- 🧰 .NET 7.0 SDK or later (https://dotnet.microsoft.com/en-us/download)
- 🎭 Microsoft.Playwright package installed
- 🌐 Chromium browser (installed automatically by Playwright)

---

1. Clone the repository:

   git clone https://github.com/yourusername/AutomationChallenge.git
   cd AutomationChallenge

---

2. Install Playwright CLI and Browsers

Microsoft.Playwright requires both the .NET package and browser binaries (Chromium).

Run these commands to install the CLI tool and the browser dependencies:

   dotnet tool install --global Microsoft.Playwright.CLI
   playwright install

---

3. Set Environment Variables

Set the following in your shell or PowerShell session:

   $env:USERNAME="your_username_or_email"
   $env:PASSWORD="your_password"
   $env:GITHUB_2FA_CODE="123456"
   $env:NEW_BIO="Your new bio text here"

Note:
- For 2FA, if the code timer expires, refresh to get a new valid code before running.
- Make sure your GitHub account is not linked to Google OAuth login; this automation does not handle Google logins.

---

4. Run the application:

   dotnet run

