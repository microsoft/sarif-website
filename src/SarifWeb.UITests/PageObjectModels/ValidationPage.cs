// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SarifWeb.PageObjectModels
{
    internal class ValidationPage : Page
    {
        private static readonly TimeSpan ValidationTimeout = TimeSpan.FromSeconds(10);

        internal ValidationPage(IWebDriver driver) : base(driver) { }

        protected override string RelativeUri => "Validation";

        internal int CurrentResultIndex =>
            int.Parse(Driver.FindElement(By.Id("resultPositionCurrent")).Text);

        internal int NumResults =>
            int.Parse(Driver.FindElement(By.Id("resultPositionCount")).Text);

        internal void ClickAdditionalSuggestions() =>
            Driver.FindElement(By.Id("noteRules")).Click();

        internal void ClickGitHubRules() =>
            Driver.FindElement(By.Id("gitHubRules")).Click();

        internal void DropFile(string path)
        {
            // Type the file path into the hidden file input element.
            const string FileInputElementId = "fileInput";
            IWebElement input = Driver.FindElement(By.Id(FileInputElementId));
            string absolutePath = Path.GetFullPath(path);
            input.SendKeys(absolutePath);

            // Now execute JavaScript to trigger the "change" event on the file Input element.
            // This starts the validation process.
            //
            // We can't just press ENTER on the Open File dialog, because it's controlled
            // by the operating system, so neither Selenium nor raw JavaScript can touch it.
            string triggerChangeEvent =
$@"const event = new Event('change');
const fileInput = document.getElementById('{FileInputElementId}');
fileInput.dispatchEvent(event);";

            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript(triggerChangeEvent);
        }

        internal void WaitForRuleListEnabled()
        {
            IWebElement ruleList = Driver.FindElement(By.Id("ruleList"));
            WebDriverWait wait = new WebDriverWait(Driver, ValidationTimeout);
            wait.Until(driver => ruleList.Enabled);
        }
    }
}
