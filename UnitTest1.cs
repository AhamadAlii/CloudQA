// DiscoverAndTestForms.cs
// Requires: Selenium.WebDriver, Selenium.Support, Selenium.WebDriver.ChromeDriver, NUnit
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CloudQATests
{
    [TestFixture]
    public class DiscoverAndTestForms
    {
        private IWebDriver? _driver;
        private WebDriverWait? _wait;
        private const string Url = "https://app.cloudqa.io/home/AutomationPracticeForm";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
            _driver.Navigate().GoToUrl(Url);
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); _driver?.Dispose(); }
            catch { }
            finally { _driver = null; _wait = null; }
        }

        [Test]
        public void ExerciseAllFormsOnPage()
        {
            var driver = _driver!;
            var wait = _wait!;

            // find all forms
            var forms = driver.FindElements(By.TagName("form"));
            Console.WriteLine($"Found {forms.Count} forms on page.");

            int idx = 0;
            foreach (var form in forms)
            {
                idx++;
                try
                {
                    Console.WriteLine($"\n--- Form #{idx} ---");

                    // bring form into view
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", form);

                    // collect fields inside form
                    var inputs = form.FindElements(By.XPath(".//input | .//select | .//textarea"));
                    Console.WriteLine($"  Fields found: {inputs.Count}");

                    // For each field choose action by type
                    foreach (var field in inputs)
                    {
                        var tag = field.TagName.ToLower();
                        var type = (field.GetAttribute("type") ?? "").ToLower();
                        var name = (field.GetAttribute("name") ?? field.GetAttribute("id") ?? "<no-name>").Trim();
                        Console.WriteLine($"   - {tag} type='{type}' name='{name}'");

                        try
                        {
                            if (tag == "input")
                            {
                                switch (type)
                                {
                                    case "text":
                                    case "search":
                                    case "email":
                                    case "tel":
                                        SafeClearAndSendKeys(field, $"test_{Sanitize(name)}");
                                        break;
                                    case "number":
                                        SafeClearAndSendKeys(field, "123");
                                        break;
                                    case "date":
                                        SafeClearAndSendKeys(field, DateTime.Today.ToString("yyyy-MM-dd"));
                                        break;
                                    case "password":
                                        SafeClearAndSendKeys(field, "P@ssw0rd!");
                                        break;
                                    case "checkbox":
                                        if (!field.Selected) field.Click();
                                        break;
                                    case "radio":
                                        // pick the radio if not yet selected
                                        if (!field.Selected) field.Click();
                                        break;
                                    case "file":
                                        Console.WriteLine("    skipping file input (unsafe)"); 
                                        break;
                                    case "hidden":
                                        // ignore
                                        break;
                                    case "submit":
                                        // don't auto-click submit here
                                        break;
                                    default:
                                        // fallback attempt
                                        SafeClearAndSendKeys(field, $"test_{Sanitize(name)}");
                                        break;
                                }
                            }
                            else if (tag == "select")
                            {
                                var select = new SelectElement(field);
                                var option = select.Options.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.GetAttribute("value")) && !o.Text.Contains("Select") );
                                if (option != null)
                                {
                                    select.SelectByValue(option.GetAttribute("value"));
                                    Console.WriteLine($"    selected option '{option.Text}'");
                                }
                            }
                            else if (tag == "textarea")
                            {
                                SafeClearAndSendKeys(field, "This is an automated test note.");
                            }
                        }
                        catch (Exception exField)
                        {
                            Console.WriteLine($"    Field action failed: {exField.Message}");
                        }
                    }

                    // Optional: attempt safe submit
                    // By default we DON'T submit (avoid external side effects). Uncomment to enable:
                    /*
                    try
                    {
                        var submit = form.FindElements(By.XPath(".//button[@type='submit'] | .//input[@type='submit']")).FirstOrDefault();
                        if (submit != null)
                        {
                            Console.WriteLine("  Attempting form submit...");
                            submit.Click();
                            wait.Until(d => d.Url != Url || d.PageSource.Length > 0);
                            Console.WriteLine("  Submit done - new URL: " + driver.Url);
                            // optionally navigate back
                            driver.Navigate().Back();
                        }
                    }
                    catch (Exception exSub) { Console.WriteLine("  Submit failed: " + exSub.Message); }
                    */

                    Console.WriteLine($"--- Form #{idx} done ---");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Form #{idx} processing threw: {ex.Message}");
                }
            }

            // If you want a strict assertion: ensure at least one form was exercised
            Assert.That(forms.Count, Is.GreaterThan(0), "No forms found on page.");
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "field";
            return string.Concat(s.Where(c => char.IsLetterOrDigit(c) || c=='_')).ToLower();
        }

        private static void SafeClearAndSendKeys(IWebElement el, string text)
        {
            try
            {
                if (!el.Displayed) return;
                el.Clear();
                el.SendKeys(text);
            }
            catch
            {
                // sometimes Clear() is not supported (e.g., custom inputs), try JS set
                try
                {
                    var driver = ((IWrapsDriver)el).WrappedDriver;
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = arguments[1];", el, text);
                }
                catch { /* swallow */ }
            }
        }
    }
}
