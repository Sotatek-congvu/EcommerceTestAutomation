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
    private readonly By _nameproduct = By.CssSelector("h2.a-size-medium.a-color-base.a-text-normal span");
    public AmazonPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public void NavigateTo()
    {
        _driver.Navigate().GoToUrl("https://www.amazon.com");
        Console.WriteLine($"Current URL after navigation: {_driver.Url}");
    }

    

   

    public List<Product> SearchForProduct(string productName)
    {
        // Enter the product name in the search box
        Task.Delay(15000).Wait();
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
       for(var i = 0; i < productname.Count; i++)
        {
            Product product = new Product();
            product.Website = "Amazon";
            product.Name = productname[i].Text;
            //add to product list
            productList.Add(product);
        }
        return productList; 
    }

   
}