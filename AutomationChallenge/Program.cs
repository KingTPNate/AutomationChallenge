using Microsoft.Playwright;

namespace AutomationChallenge
{
    internal class Program
    {
        public static async Task Main()
        {
            var username = Environment.GetEnvironmentVariable("USERNAME");
            var password = Environment.GetEnvironmentVariable("PASSWORD");
            var twoFactorCode = Environment.GetEnvironmentVariable("GITHUB_2FA_CODE");
            var newBio = Environment.GetEnvironmentVariable("NEW_BIO");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(newBio))
            {
                Console.WriteLine("Error: Missing USERNAME, PASSWORD, or NEW_BIO environment variables.");
                return;
            }

            using var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true});
            var context = await browser.NewContextAsync(new() { RecordVideoDir = "videos/" });
            var page = await context.NewPageAsync();

            // Step 1: Navigate to GitHub login
            await page.GotoAsync("https://github.com/login");

            // Step 2: Fill username & password and submit
            await page.FillAsync("input[name='login']", username);
            await page.FillAsync("input[name='password']", password);
            await page.ClickAsync("input[name='commit']");

            // Step 3: Handle 2FA if prompted
            // Note make sure github account isnt linked with google else a google login will be prompted
            try
            {
                await page.WaitForSelectorAsync("input[name='app_otp']", new PageWaitForSelectorOptions { Timeout = 5000 });

                if (string.IsNullOrEmpty(twoFactorCode))
                {
                    Console.WriteLine("2FA code required but GITHUB_2FA_CODE env variable is not set.");
                    return;
                }

                await page.FillAsync("input[name='app_otp']", twoFactorCode);

                // Click verify button and wait for navigation away from 2FA page
                await Task.WhenAll(
                    page.ClickAsync("button[type='submit']"),
                    page.WaitForURLAsync(url => !url.Contains("two-factor"), new PageWaitForURLOptions { Timeout = 15000 })
                );

                var errorMessage = await page.TextContentAsync(".flash-error");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine("2FA error message: " + errorMessage);
                    return;
                }
            }
            catch (TimeoutException)
            {
                // No 2FA prompt detected; continue
            }

            // Step 4: Wait for successful login
            await page.WaitForURLAsync(url => url.Contains("github.com/"), new PageWaitForURLOptions { Timeout = 30000 });

            // Step 5: Navigate to profile settings page to update bio
            await page.GotoAsync("https://github.com/settings/profile");

            // Take before screenshot
            await page.ScreenshotAsync(new() { Path = "before.png" });

            // Fill bio textarea
            Console.WriteLine("Filling bio...");
            await page.FillAsync("textarea#user_profile_bio", newBio);

            // Blur the textarea to trigger UI update
            await page.PressAsync("textarea#user_profile_bio", "Tab");

            // Wait for UI reactions
            await page.WaitForTimeoutAsync(2000);

            // Scroll down a bit to make the button visible if needed
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(500);

            // Debug buttons
            var buttons = await page.Locator("button.Button--primary").AllAsync();
            Console.WriteLine($"Found {buttons.Count} buttons with class Button--primary.");
            foreach (var btn in buttons)
            {
                var text = await btn.TextContentAsync();
                var visible = await btn.IsVisibleAsync();
                Console.WriteLine($"Button text: '{text}', visible: {visible}");
            }

            var saveButton = page.Locator("button.Button--primary:has-text('Update profile')");
            if (await saveButton.CountAsync() == 0)
            {
                Console.WriteLine("No 'Update profile' button found!");
                return;
            }
            if (!await saveButton.IsVisibleAsync())
            {
                Console.WriteLine("'Update profile' button found but not visible!");
                return;
            }

            Console.WriteLine("Clicking 'Update profile' button...");
            await saveButton.ClickAsync();

            // Wait for confirmation message
            await page.WaitForSelectorAsync("text=Profile updated successfully", new PageWaitForSelectorOptions { Timeout = 15000 });



            // Take after screenshot
            await page.ScreenshotAsync(new() { Path = "after.png" });

            // Print updated bio from textarea
            var currentBio = await page.InputValueAsync("textarea#user_profile_bio");
            Console.WriteLine($"Updated bio: {currentBio}");

            await browser.CloseAsync();
        }
    }
}