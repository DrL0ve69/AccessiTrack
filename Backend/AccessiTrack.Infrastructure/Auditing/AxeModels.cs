using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AccessiTrack.Infrastructure.Auditing;

internal record AxeResults(
    [property: JsonPropertyName("violations")] List<AxeRule> Violations,
    [property: JsonPropertyName("passes")] List<AxeRule> Passes,
    [property: JsonPropertyName("incomplete")] List<AxeRule> Incomplete
);

internal record AxeRule(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("impact")] string? Impact,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("helpUrl")] string HelpUrl,
    [property: JsonPropertyName("tags")] List<string> Tags,
    [property: JsonPropertyName("nodes")] List<AxeNode> Nodes
);

internal record AxeNode(
    [property: JsonPropertyName("html")] string Html,
    [property: JsonPropertyName("target")] List<string> Target,
    [property: JsonPropertyName("failureSummary")] string? FailureSummary
);
