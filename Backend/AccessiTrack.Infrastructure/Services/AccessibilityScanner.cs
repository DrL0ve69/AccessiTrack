using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AccessiTrack.Application.Violations.Commands;
using AccessiTrack.Domain.Enums;
using AccessiTrack.Application.Common.Interfaces;

namespace AccessiTrack.Infrastructure.Services;


/// <summary>
/// Scans HTML content for WCAG 2.1 accessibility violations using AngleSharp.
/// Checks: alt text, empty links, form labels, heading structure, lang attribute,
/// duplicate IDs, button names, ARIA attributes, colour contrast hints,
/// table headers, document title, and viewport meta tag.
/// </summary>
public class AccessibilityScanner : IAccessibilityScanner
{
    private readonly HttpClient _httpClient;

    public AccessibilityScanner(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<LogViolationCommand>> ScanUrlAsync(string url, CancellationToken ct = default)
    {
        var violations = new List<LogViolationCommand>();

        // Fetch HTML content
        var html = await _httpClient.GetStringAsync(url, ct);

        // Parse HTML with AngleSharp
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html), ct);

        // Run all WCAG checks
        violations.AddRange(CheckImagesAltText(document));
        violations.AddRange(CheckEmptyLinks(document));
        violations.AddRange(CheckFormLabels(document));
        violations.AddRange(CheckHeadingStructure(document));
        violations.AddRange(CheckLanguageAttribute(document));
        violations.AddRange(CheckDuplicateIds(document));
        violations.AddRange(CheckButtonAccessibleNames(document));
        violations.AddRange(CheckARIAAttributes(document));
        violations.AddRange(CheckColorContrast(document));
        violations.AddRange(CheckTableHeaders(document));
        violations.AddRange(CheckDocumentTitle(document));
        violations.AddRange(CheckMetaViewport(document));

        return violations;
    }

    /// <summary>
    /// WCAG 1.1.1 - All non-text content must have text alternatives.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckImagesAltText(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Check images without alt attribute
        var images = document.QuerySelectorAll("img");
        foreach (var img in images)
        {
            if (!img.HasAttribute("alt"))
            {
                violations.Add(CreateViolation(
                    "1.1.1",
                    "Non-text Content",
                    img.OuterHtml,
                    "Image missing alt attribute. All images must have alternative text."
                ));
            }
            else if (string.IsNullOrWhiteSpace(img.GetAttribute("alt")))
            {
                // Empty alt is only valid for decorative images
                var src = img.GetAttribute("src") ?? "unknown";
                if (!IsLikelyDecorative(img))
                {
                    violations.Add(CreateViolation(
                        "1.1.1",
                        "Non-text Content",
                        img.OuterHtml,
                        $"Image has empty alt attribute. If informative, provide descriptive text. (src: {src})"
                    ));
                }
            }
        }

        // Check SVG without accessible name
        var svgs = document.QuerySelectorAll("svg");
        foreach (var svg in svgs)
        {
            var hasAriaLabel = svg.HasAttribute("aria-label") && !string.IsNullOrWhiteSpace(svg.GetAttribute("aria-label"));
            var hasTitle = svg.QuerySelector("title") is not null;
            var hasRoleNone = svg.GetAttribute("role") == "none" || svg.GetAttribute("role") == "presentation";

            if (!hasAriaLabel && !hasTitle && !hasRoleNone)
            {
                violations.Add(CreateViolation(
                    "1.1.1",
                    "Non-text Content",
                    svg.OuterHtml,
                    "SVG missing accessible name. Provide aria-label or title element."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 2.4.4 - Link purpose must be determinable from link text alone.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckEmptyLinks(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var links = document.QuerySelectorAll("a[href]");
        foreach (var link in links)
        {
            var textContent = link.TextContent.Trim();
            var hasAriaLabel = link.HasAttribute("aria-label") && !string.IsNullOrWhiteSpace(link.GetAttribute("aria-label"));
            var hasImgWithAlt = link.QuerySelector("img[alt]") is not null;

            if (string.IsNullOrEmpty(textContent) && !hasAriaLabel && !hasImgWithAlt)
            {
                violations.Add(CreateViolation(
                    "2.4.4",
                    "Link Purpose (In Context)",
                    link.OuterHtml,
                    "Empty link. Provide descriptive text or aria-label for link purpose."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 1.3.1 & 3.3.2 - Form inputs must have associated labels.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckFormLabels(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Check inputs without labels
        var inputs = document.QuerySelectorAll("input:not([type='hidden']):not([type='submit']):not([type='button']):not([type='reset'])");
        foreach (var input in inputs)
        {
            var id = input.GetAttribute("id");
            var hasLabel = !string.IsNullOrEmpty(id) && document.QuerySelector($"label[for='{id}']") is not null;
            var hasAriaLabel = input.HasAttribute("aria-label") && !string.IsNullOrWhiteSpace(input.GetAttribute("aria-label"));
            var hasAriaLabelledBy = input.HasAttribute("aria-labelledby");
            var hasTitle = input.HasAttribute("title") && !string.IsNullOrWhiteSpace(input.GetAttribute("title"));
            var isInsideLabel = input.ParentElement?.TagName == "LABEL";

            if (!hasLabel && !hasAriaLabel && !hasAriaLabelledBy && !hasTitle && !isInsideLabel)
            {
                violations.Add(CreateViolation(
                    "1.3.1",
                    "Info and Relationships",
                    input.OuterHtml,
                    $"Form input missing label. Use <label for='id'> or aria-label. (type: {input.GetAttribute("type") ?? "text"})"
                ));
            }
        }

        // Check textareas without labels
        var textareas = document.QuerySelectorAll("textarea");
        foreach (var textarea in textareas)
        {
            var id = textarea.GetAttribute("id");
            var hasLabel = !string.IsNullOrEmpty(id) && document.QuerySelector($"label[for='{id}']") is not null;
            var hasAriaLabel = textarea.HasAttribute("aria-label");
            var isInsideLabel = textarea.ParentElement?.TagName == "LABEL";

            if (!hasLabel && !hasAriaLabel && !isInsideLabel)
            {
                violations.Add(CreateViolation(
                    "1.3.1",
                    "Info and Relationships",
                    textarea.OuterHtml,
                    "Textarea missing associated label."
                ));
            }
        }

        // Check selects without labels
        var selects = document.QuerySelectorAll("select");
        foreach (var select in selects)
        {
            var id = select.GetAttribute("id");
            var hasLabel = !string.IsNullOrEmpty(id) && document.QuerySelector($"label[for='{id}']") is not null;
            var hasAriaLabel = select.HasAttribute("aria-label");
            var isInsideLabel = select.ParentElement?.TagName == "LABEL";

            if (!hasLabel && !hasAriaLabel && !isInsideLabel)
            {
                violations.Add(CreateViolation(
                    "1.3.1",
                    "Info and Relationships",
                    select.OuterHtml,
                    "Select dropdown missing associated label."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 2.4.6 - Headings and labels should describe topic or purpose.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckHeadingStructure(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Check for empty headings
        var headings = document.QuerySelectorAll("h1, h2, h3, h4, h5, h6");
        foreach (var heading in headings)
        {
            var textContent = heading.TextContent.Trim();
            var hasAriaLabel = heading.HasAttribute("aria-label");

            if (string.IsNullOrEmpty(textContent) && !hasAriaLabel)
            {
                violations.Add(CreateViolation(
                    "2.4.6",
                    "Headings and Labels",
                    heading.OuterHtml,
                    $"Empty heading ({heading.TagName}). Headings must contain descriptive text."
                ));
            }
        }

        // Check for skipped heading levels (e.g., h1 -> h3)
        var headingList = headings.ToList();
        if (headingList.Count > 0)
        {
            int? lastLevel = null;
            foreach (var heading in headingList)
            {
                var currentLevel = int.Parse(heading.TagName[1].ToString());

                if (lastLevel.HasValue && currentLevel > lastLevel.Value + 1)
                {
                    violations.Add(CreateViolation(
                        "1.3.1",
                        "Info and Relationships",
                        heading.OuterHtml,
                        $"Skipped heading level (h{lastLevel} -> h{currentLevel}). Heading levels should not be skipped."
                    ));
                }

                lastLevel = currentLevel;
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 3.1.1 - Default human language of page must be declared.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckLanguageAttribute(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var htmlElement = document.QuerySelector("html");
        if (htmlElement is null || !htmlElement.HasAttribute("lang") || string.IsNullOrWhiteSpace(htmlElement.GetAttribute("lang")))
        {
            violations.Add(CreateViolation(
                "3.1.1",
                "Language of Page",
                document.DocumentElement?.OuterHtml ?? "<html>",
                "HTML element missing lang attribute. Declare the page's primary language (e.g., lang='en' or lang='fr')."
            ));
        }

        return violations;
    }

    /// <summary>
    /// WCAG 4.1.1 - IDs must be unique.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckDuplicateIds(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var allElements = document.QuerySelectorAll("*[id]");
        var idCounts = new Dictionary<string, int>();

        foreach (var el in allElements)
        {
            var id = el.GetAttribute("id");
            if (!string.IsNullOrEmpty(id))
            {
                if (idCounts.ContainsKey(id))
                    idCounts[id]++;
                else
                    idCounts[id] = 1;
            }
        }

        foreach (var kvp in idCounts.Where(x => x.Value > 1))
        {
            var element = document.QuerySelector($"*[id='{kvp.Key}']");
            violations.Add(CreateViolation(
                "4.1.1",
                "Parsing",
                element?.OuterHtml ?? $"*[id='{kvp.Key}']",
                $"Duplicate ID found: '{kvp.Key}'. All ID attributes must be unique."
            ));
        }

        return violations;
    }

    /// <summary>
    /// WCAG 4.1.2 - Buttons must have accessible names.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckButtonAccessibleNames(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Check buttons without accessible names
        var buttons = document.QuerySelectorAll("button");
        foreach (var button in buttons)
        {
            var textContent = button.TextContent.Trim();
            var hasAriaLabel = button.HasAttribute("aria-label") && !string.IsNullOrWhiteSpace(button.GetAttribute("aria-label"));
            var hasAriaLabelledBy = button.HasAttribute("aria-labelledby");
            var hasTitle = button.HasAttribute("title") && !string.IsNullOrWhiteSpace(button.GetAttribute("title"));

            if (string.IsNullOrEmpty(textContent) && !hasAriaLabel && !hasAriaLabelledBy && !hasTitle)
            {
                violations.Add(CreateViolation(
                    "4.1.2",
                    "Name, Role, Value",
                    button.OuterHtml,
                    "Button missing accessible name. Add text content, aria-label, or aria-labelledby."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 4.1.2 - Check for proper ARIA attribute usage.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckARIAAttributes(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Elements with role but missing required ARIA attributes
        var ariaRoles = document.QuerySelectorAll("[role]");
        foreach (var el in ariaRoles)
        {
            var role = el.GetAttribute("role");

            // Check for empty or invalid roles
            if (string.IsNullOrWhiteSpace(role))
            {
                violations.Add(CreateViolation(
                    "4.1.2",
                    "Name, Role, Value",
                    el.OuterHtml,
                    "Empty role attribute. Remove or specify a valid ARIA role."
                ));
            }
        }

        // Check for aria-expanded without valid value
        var expandedElements = document.QuerySelectorAll("[aria-expanded]");
        foreach (var el in expandedElements)
        {
            var expanded = el.GetAttribute("aria-expanded");
            if (expanded != "true" && expanded != "false")
            {
                violations.Add(CreateViolation(
                    "4.1.2",
                    "Name, Role, Value",
                    el.OuterHtml,
                    "Invalid aria-expanded value. Must be 'true' or 'false'."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 1.4.3 - Check for potential contrast issues.
    /// Note: Full contrast checking requires rendering; this checks for inline styles
    /// that might indicate contrast issues.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckColorContrast(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        // Check for elements with both color and background-color inline styles
        // This is a basic heuristic - full contrast checking requires rendering
        var elementsWithColor = document.QuerySelectorAll("[style*='color']");

        foreach (var el in elementsWithColor)
        {
            var style = el.GetAttribute("style") ?? "";
            var hasColor = style.Contains("color:", StringComparison.OrdinalIgnoreCase);
            var hasBgColor = style.Contains("background", StringComparison.OrdinalIgnoreCase);

            // Flag small text with potential contrast issues
            if (hasColor && hasBgColor && el.TagName is "span" or "p" or "a" or "td")
            {
                // We can't calculate actual contrast without rendering
                // Just warn about inline color styles which may have issues
                violations.Add(CreateViolation(
                    "1.4.3",
                    "Contrast (Minimum)",
                    el.OuterHtml,
                    "Inline color styles detected. Verify text has at least 4.5:1 contrast ratio against background."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 1.3.1 - Tables should have headers.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckTableHeaders(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var tables = document.QuerySelectorAll("table");
        foreach (var table in tables)
        {
            var hasTh = table.QuerySelector("th") is not null;
            var hasScope = table.QuerySelector("th[scope]") is not null;

            if (!hasTh)
            {
                violations.Add(CreateViolation(
                    "1.3.1",
                    "Info and Relationships",
                    table.OuterHtml,
                    "Table missing header cells (<th>). Use <th> elements to identify row/column headers."
                ));
            }
        }

        return violations;
    }

    /// <summary>
    /// WCAG 2.4.2 - Pages should have a title.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckDocumentTitle(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var titleElement = document.QuerySelector("title");
        if (titleElement is null || string.IsNullOrWhiteSpace(titleElement.TextContent))
        {
            violations.Add(CreateViolation(
                "2.4.2",
                "Page Titled",
                "<head>",
                "Document missing or empty <title> element. Every page needs a descriptive title."
            ));
        }

        return violations;
    }

    /// <summary>
    /// WCAG 1.4.10 - Reflow should not require horizontal scrolling.
    /// Check for fixed-width styles that might cause issues.
    /// </summary>
    private static IEnumerable<LogViolationCommand> CheckMetaViewport(IDocument document)
    {
        var violations = new List<LogViolationCommand>();

        var viewport = document.QuerySelector("meta[name='viewport']");
        if (viewport is null)
        {
            violations.Add(CreateViolation(
                "1.4.10",
                "Reflow",
                "<head>",
                "Missing viewport meta tag. Add <meta name='viewport' content='width=device-width, initial-scale=1'> for responsive design."
            ));
        }
        else
        {
            var content = viewport.GetAttribute("content");
            if (!string.IsNullOrEmpty(content) && content.Contains("user-scalable=no"))
            {
                violations.Add(CreateViolation(
                    "1.4.10",
                    "Reflow",
                    viewport.OuterHtml,
                    "Viewport disables user scaling. Allow users to zoom for readability."
                ));
            }
        }

        return violations;
    }

    private static LogViolationCommand CreateViolation(
        string wcagCriterion,
        string wcagCriterionName,
        string htmlElement,
        string description)
    {
        // Determine severity based on WCAG level and impact
        var severity = DetermineSeverity(wcagCriterion);

        return new LogViolationCommand(
            Guid.Empty, // Will be set by handler
            wcagCriterion,
            wcagCriterionName,
            TruncateElement(htmlElement),
            description,
            severity
        );
    }

    private static ViolationSeverity DetermineSeverity(string wcagCriterion)
    {
        // Level A violations are Critical
        // Level AA violations are Major
        // Level AAA or best practice are Minor

        var levelACriteria = new[] { "1.1.1", "1.3.1", "2.1.1", "2.4.4", "3.1.1", "4.1.1", "4.1.2" };
        var levelAACriteria = new[] { "1.4.3", "1.4.10", "2.4.6", "3.1.2" };

        if (levelACriteria.Contains(wcagCriterion))
            return ViolationSeverity.Critical;

        if (levelAACriteria.Contains(wcagCriterion))
            return ViolationSeverity.Major;

        return ViolationSeverity.Minor;
    }

    private static bool IsLikelyDecorative(IElement element)
    {
        // Heuristics to determine if an image is likely decorative
        var src = element.GetAttribute("src") ?? "";

        // Spacer images, tracking pixels, etc.
        if (src.Contains("spacer") || src.Contains("pixel") || src.Contains("blank") ||
            src.Contains("1x1") || src.Contains("transparent") || src.StartsWith("data:image/gif"))
            return true;

        // Images inside links that are likely icons
        if (element.ParentElement?.TagName == "A" && element.ClassList.Contains("icon"))
            return true;

        return false;
    }

    private static string TruncateElement(string html, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(html) || html.Length <= maxLength)
            return html;

        return html[..maxLength] + "...";
    }
}
