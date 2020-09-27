// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using OpenQA.Selenium;

namespace SarifWeb.PageObjectModels
{
    internal abstract class Page
    {
        // IMPORTANT: This must match the element
        // /Project/ProjectExtensions/VisualStudio/FlavorProperties/WebProjectProperties/DevelopmentServerPort
        // in SarifWeb.csproj.
        //
        // Unlike in an ASP.NET Core Web project, there is no launchSettings.json file to hold the
        // startup URL. Rather, Visual Studio creates a file .vs\config\applicationhost.config from
        // the project settings, and uses that to launch the app. But this file is not available in
        // the running app's bin folder. I don't know of a way to determine the startup URL at
        // runtime from _outside_ the running app.
        private const int Port = 28402;

        protected Page(IWebDriver driver)
        {
            Driver = driver;
        }

        protected abstract string RelativeUri { get; }

        protected IWebDriver Driver { get; }

        public string PageUri => $"http://localhost:{Port}/{RelativeUri}";

        internal void NavigateTo()
        {
            Driver.Navigate().GoToUrl(PageUri);
            EnsurePageLoaded();
        }

        private void EnsurePageLoaded()
        {
            Driver.Url.Should().Be(PageUri);
        }
    }
}
