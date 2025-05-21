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
            var amazonProducts = amazonPage.SearchForProduct("Iphone 16");
            Console.WriteLine("Retrieved products from Amazon:");
            

            Assert.IsTrue(amazonProducts.Count()>0);
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