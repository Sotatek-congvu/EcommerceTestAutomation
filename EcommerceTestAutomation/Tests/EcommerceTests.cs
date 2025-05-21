using EcommerceTestAutomation.Models;
using EcommerceTestAutomation.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;

namespace EcommerceTestAutomation.Tests;

[TestFixture]
public class EcommerceTests
{
    private IWebDriver _driver;

    [SetUp]
    public void Setup()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10); // Giảm xuống 10 giây để tránh treo lâu
    }

    [Test]
    public void SearchIPhone16OnAmazon()
    {
        try
        {
            var amazonPage = new AmazonPage(_driver);
            Console.WriteLine("Navigating to Amazon...");
            amazonPage.NavigateTo();
            Console.WriteLine("Searching for 'iPhone 16' on Amazon...");
            amazonPage.SearchForProduct("iPhone 16");
            var amazonProducts = amazonPage.GetSearchResults();
            Console.WriteLine("Retrieved products from Amazon:");
            foreach (var product in amazonProducts)
            {
                Console.WriteLine($"  - Website: {product.Website}, Product: {product.Name}, Price: ${product.Price}, Link: {product.Link}");
            }

            // Kiểm tra kết quả không rỗng
            Assert.IsTrue(amazonProducts.Any(), "No products found on Amazon.");
            Console.WriteLine("Test passed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
        finally
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}