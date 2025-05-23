using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using EcommerceTestAutomation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using Tesseract;
using System.Threading;

namespace EcommerceTestAutomation.Pages
{
    public class AmazonPage
    {
        private readonly IWebDriver _driver;
        private readonly By _searchBox = By.Id("twotabsearchtextbox");
        private readonly By _searchButton = By.Id("nav-search-submit-button");
        private readonly By _searchResults = By.CssSelector("div.s-main-slot div.s-result-item");
        private readonly By _nameproduct = By.CssSelector("h2.a-size-medium.a-color-base.a-text-normal span");
        private readonly By _captchaImage = By.CssSelector("div.a-row.a-text-center img");
        private readonly By _captchaInput = By.Id("captchacharacters");
        private readonly By _captchaSubmit = By.CssSelector("button.a-button-text");
        private readonly By _captchaRefresh = By.CssSelector("a[href*='reload-captcha']");

        public AmazonPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void NavigateTo()
        {
            _driver.Navigate().GoToUrl("https://www.amazon.com");
            Console.WriteLine($"Current URL after navigation: {_driver.Url}");

            if (IsCaptchaPresent())
            {
                HandleCaptcha();
            }
        }

        private bool IsCaptchaPresent()
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(1));
                wait.Until(d => d.FindElement(_captchaImage));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        private void HandleCaptcha()
        {
            Console.WriteLine("CAPTCHA detected. Trying multiple approaches...");

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Console.WriteLine($"CAPTCHA attempt {attempt}/3");

                SolveCaptchaAdvanced();

                if (!IsCaptchaPresent())
                {
                    Console.WriteLine("CAPTCHA solved successfully!");
                    return;
                }

                if (attempt < 3)
                {
                    RefreshCaptcha();
                }
            }

            RequestManualCaptchaSolving();
        }

        private void SolveCaptchaAdvanced()
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                var captchaImageElement = wait.Until(d => d.FindElement(_captchaImage));
                string captchaUrl = captchaImageElement.GetAttribute("src");
                Console.WriteLine("CAPTCHA image URL: " + captchaUrl);

                using (var client = new HttpClient())
                {
                    byte[] imageBytes = client.GetByteArrayAsync(captchaUrl).Result;
                    string tempImagePath = Path.Combine(Path.GetTempPath(), $"captcha_{DateTime.Now.Ticks}.png");

                    try
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            // Try multiple processing methods
                            var processedImages = new List<Bitmap>
                            {
                                ProcessImageMethod1(originalImage), // High contrast
                                ProcessImageMethod2(originalImage), // Edge detection
                                ProcessImageMethod3(originalImage), // Noise reduction
                                ProcessImageMethod4(originalImage)  // Color filtering
                            };

                            // Try Tesseract with different configurations
                            string[] ocrModes = { "6", "7", "8", "10" }; // Different OCR modes
                            string[] whitelists = {
                                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                                "abcdefghijklmnopqrstuvwxyz",
                                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
                            };

                            for (int i = 0; i < processedImages.Count; i++)
                            {
                                string imagePath = Path.Combine(Path.GetTempPath(), $"captcha_processed_{i}_{DateTime.Now.Ticks}.png");
                                processedImages[i].Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                                foreach (string mode in ocrModes)
                                {
                                    foreach (string whitelist in whitelists)
                                    {
                                        string result = TryTesseractWithConfig(imagePath, mode, whitelist);
                                        if (!string.IsNullOrWhiteSpace(result) && result.Length >= 4 && result.Length <= 8)
                                        {
                                            Console.WriteLine($"CAPTCHA candidate: {result} (method {i}, mode {mode})");

                                            SubmitCaptcha(result);

                                            // Check if submission was successful
                                            Thread.Sleep(1000);
                                            if (!IsCaptchaPresent())
                                            {
                                                // Cleanup and return on success
                                                foreach (var img in processedImages) img?.Dispose();
                                                CleanupTempFiles(tempImagePath, imagePath);
                                                return;
                                            }
                                        }
                                    }
                                }

                                File.Delete(imagePath);
                                processedImages[i]?.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        CleanupTempFiles(tempImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Advanced CAPTCHA solving failed: " + ex.Message);
            }
        }

        private Bitmap ProcessImageMethod1(Bitmap original)
        {
            // High contrast and noise reduction
            var result = new Bitmap(original.Width * 2, original.Height * 2); // Scale up

            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, result.Width, result.Height);
            }

            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    Color pixel = result.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);

                    // Aggressive thresholding
                    gray = gray > 100 ? 255 : 0;

                    Color newColor = Color.FromArgb(gray, gray, gray);
                    result.SetPixel(x, y, newColor);
                }
            }

            return result;
        }

        private Bitmap ProcessImageMethod2(Bitmap original)
        {
            var result = new Bitmap(original.Width, original.Height);

            for (int x = 1; x < original.Width - 1; x++)
            {
                for (int y = 1; y < original.Height - 1; y++)
                {
                    // Simple edge detection
                    Color center = original.GetPixel(x, y);
                    Color right = original.GetPixel(x + 1, y);
                    Color bottom = original.GetPixel(x, y + 1);

                    int grayCenter = (int)(center.R * 0.299 + center.G * 0.587 + center.B * 0.114);
                    int grayRight = (int)(right.R * 0.299 + right.G * 0.587 + right.B * 0.114);
                    int grayBottom = (int)(bottom.R * 0.299 + bottom.G * 0.587 + bottom.B * 0.114);

                    int edge = Math.Abs(grayCenter - grayRight) + Math.Abs(grayCenter - grayBottom);
                    edge = edge > 50 ? 0 : 255; // Invert for text

                    Color newColor = Color.FromArgb(edge, edge, edge);
                    result.SetPixel(x, y, newColor);
                }
            }

            return result;
        }

        private Bitmap ProcessImageMethod3(Bitmap original)
        {
            // Color filtering - focus on dark text
            var result = new Bitmap(original.Width, original.Height);

            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color pixel = original.GetPixel(x, y);

                    // Filter based on color similarity to typical text
                    bool isDarkEnough = pixel.R < 150 && pixel.G < 150 && pixel.B < 150;
                    bool isNotBackground = Math.Abs(pixel.R - pixel.G) < 50 && Math.Abs(pixel.G - pixel.B) < 50;

                    int value = (isDarkEnough && isNotBackground) ? 0 : 255;
                    Color newColor = Color.FromArgb(value, value, value);
                    result.SetPixel(x, y, newColor);
                }
            }

            return result;
        }

        private Bitmap ProcessImageMethod4(Bitmap original)
        {
            // Adaptive thresholding
            var result = new Bitmap(original.Width, original.Height);
            int[,] grayValues = new int[original.Width, original.Height];

            // Convert to grayscale first
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color pixel = original.GetPixel(x, y);
                    grayValues[x, y] = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                }
            }

            // Adaptive threshold
            int windowSize = 15;
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    int sum = 0, count = 0;

                    for (int dx = -windowSize / 2; dx <= windowSize / 2; dx++)
                    {
                        for (int dy = -windowSize / 2; dy <= windowSize / 2; dy++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && nx < original.Width && ny >= 0 && ny < original.Height)
                            {
                                sum += grayValues[nx, ny];
                                count++;
                            }
                        }
                    }

                    int threshold = sum / count - 10; // Slightly bias toward text
                    int value = grayValues[x, y] < threshold ? 0 : 255;
                    Color newColor = Color.FromArgb(value, value, value);
                    result.SetPixel(x, y, newColor);
                }
            }

            return result;
        }

        private string TryTesseractWithConfig(string imagePath, string ocrMode, string whitelist)
        {
            try
            {
                string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessDataPath)) return null;

                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_ocr_engine_mode", ocrMode);
                    engine.SetVariable("tessedit_char_whitelist", whitelist);
                    engine.SetVariable("tessedit_pageseg_mode", "8"); // Single word

                    using (var img = Pix.LoadFromFile(imagePath))
                    using (var page = engine.Process(img))
                    {
                        return page.GetText().Trim().Replace(" ", "").Replace("\n", "");
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void SubmitCaptcha(string captchaText)
        {
            try
            {
                var captchaInput = _driver.FindElement(_captchaInput);
                captchaInput.Clear();
                captchaInput.SendKeys(captchaText);

                var captchaSubmit = _driver.FindElement(_captchaSubmit);
                captchaSubmit.Click();

                Console.WriteLine($"Submitted CAPTCHA text: {captchaText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to submit CAPTCHA: {ex.Message}");
            }
        }

        private void RefreshCaptcha()
        {
            try
            {
                var refreshButton = _driver.FindElement(_captchaRefresh);
                refreshButton.Click();
                Thread.Sleep(1000);
            }
            catch
            {
                _driver.Navigate().Refresh();
                Thread.Sleep(2000);
            }
        }

        private void RequestManualCaptchaSolving()
        {
            Console.WriteLine("\n=== MANUAL CAPTCHA SOLVING REQUIRED ===");
            Console.WriteLine("Automated CAPTCHA solving failed.");
            Console.WriteLine("Please solve the CAPTCHA manually in the browser.");
            Console.WriteLine("Press ENTER when you have solved the CAPTCHA...");
            Console.ReadLine();

            Console.WriteLine("Manual CAPTCHA solving completed.");
        }

        private void CleanupTempFiles(params string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch { }
            }
        }

        public List<Product> SearchForProduct(string productName)
        {
            Task.Delay(10000).Wait();
            var searchBox = _driver.FindElement(_searchBox);
            searchBox.Clear();
            searchBox.SendKeys(productName);
            // Click the search button
            var searchButton = _driver.FindElement(_searchButton);
            searchButton.Click();
            // Wait for the search results to load
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(1));
            wait.Until(d => d.FindElements(_searchResults).Count > 0);
            var productname = _driver.FindElements(_nameproduct);
            var productList = new List<Product>();
            for (var i = 0; i < productname.Count; i++)
            {
                Product product = new Product();
                product.Website = "Amazon";
                product.Name = productname[i].Text;
                //add to product list
                productList.Add(product);
            }
            SaveProductsToFile(productList);
            return productList;
        }

        private void SaveProductsToFile(List<Product> products)
        {
            try
            {
                string filePath = "C:\\Users\\ACER\\source\\repos\\EcommerceTestAutomation\\EcommerceTestAutomation\\Tests\\Product.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"Search performed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    foreach (var product in products)
                    {
                        writer.WriteLine($"Name: {product.Name}");
                        writer.WriteLine($"Website: {product.Website}");
                        writer.WriteLine("---");
                    }
                    writer.WriteLine();
                }
                Console.WriteLine($"Products saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving products to file: {ex.Message}");
            }
        }
    }
}