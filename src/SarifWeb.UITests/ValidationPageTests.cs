// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using SarifWeb.PageObjectModels;
using SarifWeb.TestUtilities;
using Xunit;

namespace SarifWeb.UITests
{
    public class ValidationPageTests : PageTestBase
    {
        [Fact]
        [Trait(TestTraits.Category, TestCategories.Smoke)]
        [Trait(TestTraits.Category, TestCategories.UITest)]
        public void ValidationPage_WhenFileIsDropped_ShouldDisplayExpectedResults()
        {
            const string TestFilePath = @"TestData\Test.sarif";

            var page = new ValidationPage(Driver);
            page.NavigateTo();

            // Page initially shows no results.
            page.NumResults.Should().Be(0);
            page.CurrentResultIndex.Should().Be(0);

            page.DropFile(TestFilePath);
            page.WaitForRuleListEnabled();

            // The following explicit counts should be obtained by running the Multitool library's
            // ValidateCommand against the test file.
            page.NumResults.Should().Be(3);
            page.CurrentResultIndex.Should().Be(1);

            page.ClickAdditionalSuggestions();

            page.NumResults.Should().Be(6);
            page.CurrentResultIndex.Should().Be(1);

            page.ClickGitHubRules();

            page.NumResults.Should().Be(8);
            page.CurrentResultIndex.Should().Be(1);

            page.ClickAdditionalSuggestions();

            page.NumResults.Should().Be(6);
            page.CurrentResultIndex.Should().Be(1);
        }
    }
}
