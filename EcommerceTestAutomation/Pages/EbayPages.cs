using OpenQA.Selenium;
using EcommerceTestAutomation.Models;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceTestAutomation.Pages
{
    public class EbayPage
    {
        private readonly IWebDriver _driver;
        private readonly By _searchBox = By.Id("gh-ac");
        private readonly By _searchButton = By.Id("gh-btn");
        private readonly By _searchResults = By.CssSelector("ul.srp-results li.s-item");

        public EbayPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void NavigateTo()
        {
            _driver.Navigate().GoToUrl("https://www.ebay.com");
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

            foreach (var result in results.Take(5)) // Lấy 5 sản phẩm đầu tiên để minh họa
            {
                try
                {
                    var nameElement = result.FindElement(By.CssSelector("h3.s-item__title"));
                    var priceElement = result.FindElement(By.CssSelector("span.s-item__price"));
                    var linkElement = result.FindElement(By.CssSelector("a.s-item__link"));

                    var priceText = priceElement.Text.Replace(",", "").Replace("$", "").Split(" ")[0];
                    if (decimal.TryParse(priceText, out decimal price))
                    {
                        products.Add(new Product
                        {
                            Website = "eBay",
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
}