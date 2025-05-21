using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using EcommerceTestAutomation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceTestAutomation.Pages;

public class AmazonPage
{
    private readonly IWebDriver _driver;
    private readonly By _searchBox = By.Id("twotabsearchtextbox");
    private readonly By _searchButton = By.Id("nav-search-submit-button");
    private readonly By _searchResults = By.CssSelector("div.s-main-slot div.s-result-item");
    private readonly By _captchaImage = By.CssSelector("div.a-row img");
    private readonly By _captchaInput = By.Id("captchacharacters");
    private readonly By _captchaSubmitButton = By.CssSelector("button[type='submit']");
    private readonly By _tryDifferentImage = By.LinkText("Try different image");

    public AmazonPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public void NavigateTo()
    {
        _driver.Navigate().GoToUrl("https://www.amazon.com");
        Console.WriteLine($"Current URL after navigation: {_driver.Url}");
        HandleCaptchaIfPresent();
    }

    private bool IsCaptchaPresent()
    {
        try
        {
            return _driver.FindElements(_captchaInput).Any();
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private string GetUserInputCaptcha()
    {
        Console.WriteLine("CAPTCHA detected. Please enter the characters you see in the CAPTCHA image:");
        return Console.ReadLine();
    }

    private void HandleCaptchaIfPresent()
    {
        int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (IsCaptchaPresent())
            {
                try
                {
                    string captchaText = GetUserInputCaptcha();

                    var captchaInputField = _driver.FindElement(_captchaInput);
                    captchaInputField.Clear();
                    captchaInputField.SendKeys(captchaText);

                    _driver.FindElement(_captchaSubmitButton).Click();

                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(driver => !IsCaptchaPresent());

                    Console.WriteLine($"Current URL after CAPTCHA: {_driver.Url}");
                    break;
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine($"Attempt {attempt} failed: CAPTCHA not resolved. Trying a different image...");
                    if (attempt < maxAttempts)
                    {
                        _driver.FindElement(_tryDifferentImage).Click();
                    }
                    else
                    {
                        throw new Exception("Max CAPTCHA attempts reached. Please check the test setup or network.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"CAPTCHA handling failed: {ex.Message}");
                }
            }
        }
    }

    public void SearchForProduct(string productName)
    {
        try
        {
            var searchBox = _driver.FindElement(_searchBox);
            Console.WriteLine("Search box found.");
            searchBox.SendKeys(productName);

            var searchButton = _driver.FindElement(_searchButton);
            Console.WriteLine("Search button found.");
            searchButton.Click();

            // Wait for the search results page to load
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.Url.Contains("k=" + Uri.EscapeDataString(productName)));

            Console.WriteLine($"Current URL after search: {_driver.Url}");
        }
        catch (NoSuchElementException ex)
        {
            Console.WriteLine($"Failed to perform search: {ex.Message}");
            throw;
        }
        catch (WebDriverTimeoutException ex)
        {
            Console.WriteLine($"Search page did not load correctly: {ex.Message}");
            throw;
        }

        HandleCaptchaIfPresent();
    }

    public List<Product> GetSearchResults()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => driver.FindElements(_searchResults).Count > 0);

        var results = _driver.FindElements(_searchResults);
        Console.WriteLine($"Found {results.Count} search results.");

        var products = new List<Product>();
        foreach (var result in results.Take(5))
        {
            try
            {
                var nameElement = wait.Until(driver => result.FindElement(By.CssSelector("h2 a span")));
                var linkElement = wait.Until(driver => result.FindElement(By.CssSelector("h2 a")));

                decimal price = 0;
                try
                {
                    var priceElement = wait.Until(driver => result.FindElement(By.CssSelector("span.a-price-whole")));
                    var priceText = priceElement.Text.Replace(",", "").Replace("$", "");
                    decimal.TryParse(priceText, out price);
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine($"Price not found for product: {nameElement.Text}");
                    continue;
                }

                string link = linkElement.GetAttribute("href") ?? string.Empty;

                products.Add(new Product
                {
                    Website = "Amazon",
                    Name = nameElement.Text,
                    Price = price,
                    Link = link
                });
            }
            catch (StaleElementReferenceException)
            {
                Console.WriteLine("Stale element detected. Retrying for this result...");
                continue;
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error parsing product: {ex.Message}");
                continue;
            }
        }

        return products;
    }
}