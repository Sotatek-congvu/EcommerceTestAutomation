using OpenQA.Selenium;
using EcommerceTestAutomation.Models;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceTestAutomation.Pages;

public class AmazonPage
{
    private readonly IWebDriver _driver;
    private readonly By _searchBox = By.Id("twotabsearchtextbox");
    private readonly By _searchButton = By.Id("nav-search-submit-button");
    private readonly By _searchResults = By.CssSelector("div.s-main-slot div.s-result-item");

    public AmazonPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public void NavigateTo()
    {
        _driver.Navigate().GoToUrl("https://www.amazon.com");
    }

    public void SearchForProduct(string productName)
    {
        _driver.FindElement(_searchBox).SendKeys(productName);
        _driver.FindElement(_searchButton).Click();
    }

    public List<Product> GetSearchResults()
    {
        var results = _driver.FindElements(_searchResults);
        var products = new List<Product>();

        foreach (var result in results.Take(5))
        {
            try
            {
                var nameElement = result.FindElement(By.CssSelector("h2 a span"));
                var priceElement = result.FindElement(By.CssSelector("span.a-price-whole"));
                var linkElement = result.FindElement(By.CssSelector("h2 a"));

                var priceText = priceElement.Text.Replace(",", "").Replace("$", "");
                if (decimal.TryParse(priceText, out decimal price))
                {
                    products.Add(new Product
                    {
                        Website = "Amazon",
                        Name = nameElement.Text,
                        Price = price,
                        Link = linkElement.GetAttribute("href")
                    });
                }
            }
            catch (NoSuchElementException) { }
        }

        return products;
    }
}