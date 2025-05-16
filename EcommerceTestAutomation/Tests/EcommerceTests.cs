using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using EcommerceTestAutomation.Models;
using EcommerceTestAutomation.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EcommerceTestAutomation.Tests
{
    [TestFixture]
    public class EcommerceTests
    {
        private IWebDriver _driver;
        private ExtentReports _extent;
        private ExtentTest _test;

        [OneTimeSetUp]
        public void SetupReport()
        {
            var dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Reports");
            Directory.CreateDirectory(dir);
            var htmlReporter = new ExtentSparkReporter(Path.Combine(dir, "TestReport.html"));
            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Environment", "Windows");
            _extent.AddSystemInfo("Selenium Version", "4.25.0");
            _extent.AddSystemInfo("Language", "C#");
        }

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _test = _extent.CreateTest("Search iPhone 16 Test");
        }

        [Test]
        public void SearchIPhone16AndComparePrices()
        {
            try
            {
                // Tìm kiếm trên Amazon
                var amazonPage = new AmazonPage(_driver);
                amazonPage.NavigateTo();
                amazonPage.SearchForProduct("iPhone 16");
                var amazonProducts = amazonPage.GetSearchResults();
                var amazonScreenshot = TakeScreenshot(_driver, "Amazon_Search");
                _test.Log(Status.Info, "Retrieved products from Amazon", MediaEntityBuilder.CreateScreenCaptureFromPath(amazonScreenshot).Build());

                // Tìm kiếm trên eBay
                var ebayPage = new EbayPage(_driver);
                ebayPage.NavigateTo();
                ebayPage.SearchForProduct("iPhone 16");
                var ebayProducts = ebayPage.GetSearchResults();
                var ebayScreenshot = TakeScreenshot(_driver, "eBay_Search");
                _test.Log(Status.Info, "Retrieved products from eBay", MediaEntityBuilder.CreateScreenCaptureFromPath(ebayScreenshot).Build());

                // Kết hợp và sắp xếp kết quả
                var allProducts = amazonProducts.Concat(ebayProducts)
                    .OrderBy(p => p.Price)
                    .ToList();

                // Ghi kết quả ra console và báo cáo
                Console.WriteLine("Search Results (Sorted by Price):");
                foreach (var product in allProducts)
                {
                    var result = $"Website: {product.Website}, Product: {product.Name}, Price: ${product.Price}, Link: {product.Link}";
                    Console.WriteLine(result);
                    _test.Log(Status.Info, result);
                }

                // Kiểm tra kết quả không rỗng
                Assert.IsTrue(allProducts.Any(), "No products found.");
                _test.Pass("Test passed successfully.");
            }
            catch (Exception ex)
            {
                var errorScreenshot = TakeScreenshot(_driver, "Error");
                _test.Fail($"Test failed: {ex.Message}", MediaEntityBuilder.CreateScreenCaptureFromPath(errorScreenshot).Build());
                throw;
            }
        }

        private string TakeScreenshot(IWebDriver driver, string fileName)
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Reports", $"{fileName}.png");
            screenshot.SaveAsFile(path);
            return path;
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

        [OneTimeTearDown]
        public void CloseReport()
        {
            _extent.Flush();
        }
    }
}