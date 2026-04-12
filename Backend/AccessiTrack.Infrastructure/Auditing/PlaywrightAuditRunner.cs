using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Enums;
using AccessiTrack.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AccessiTrack.Infrastructure.Auditing;

public class PlaywrightAuditRunner(
    IAuditRepository auditRepository,
    IViolationRepository violationRepository,
    ILogger<PlaywrightAuditRunner> logger)
    : IAuditRunner
{
    private const string AxeCdn =
        "https://cdnjs.cloudflare.com/ajax/libs/axe-core/4.9.1/axe.min.js";

    // axe.run() returns a Promise — must use async IIFE
    private const string RunScript =
        "async () => { const r = await axe.run(); return JSON.stringify(r); }";

    public async Task RunAsync(Guid auditId, string url, CancellationToken ct = default)
    {
        var audit = await auditRepository.GetByIdAsync(auditId, ct)
            ?? throw new InvalidOperationException($"Audit {auditId} not found.");

        audit.MarkInProgress();
        await auditRepository.UpdateAsync(audit, ct);

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-dev-shm-usage"]
            });

            var page = await browser.NewPageAsync();

            // Navigate; NetworkIdle ensures SPAs have rendered
            await page.GotoAsync(url, new()
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            // Inject axe-core from CDN then wait for it to be ready
            await page.AddScriptTagAsync(new() { Url = AxeCdn });
            await page.WaitForFunctionAsync("() => typeof axe !== 'undefined'",
                new() ,new(){ Timeout = 10_000 });

            var json = await page.EvaluateAsync<string>(RunScript)
                ?? throw new InvalidOperationException("axe.run() returned null.");

            var results = JsonSerializer.Deserialize<AxeResults>(json)
                ?? throw new InvalidOperationException("Failed to deserialize axe results.");

            var violations = BuildViolations(auditId, results.Violations);
            var score = CalculateScore(results.Violations, results.Passes.Count);

            await violationRepository.AddRangeAsync(violations, ct);

            audit.Complete(score, violations.Count, results.Passes.Count);
            await auditRepository.UpdateAsync(audit, ct);

            logger.LogInformation(
                "Audit {AuditId} completed — score: {Score}, violations: {Count}",
                auditId, score, violations.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Audit {AuditId} failed for {Url}", auditId, url);
            audit.Fail(ex.Message);
            await auditRepository.UpdateAsync(audit, ct);
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static List<Violation> BuildViolations(
        Guid auditId, IEnumerable<AxeRule> axeViolations)
    {
        var list = new List<Violation>();

        foreach (var rule in axeViolations)
        {
            var severity = MapSeverity(rule.Impact);
            var wcag = ExtractWcag(rule.Tags);

            // One Violation row per affected node
            foreach (var node in rule.Nodes)
            {
                list.Add(Violation.Report(
                    auditId,
                    rule.Id,
                    node.FailureSummary ?? string.Empty,
                    node.Html,
                    rule.Description,
                    severity));
            }
        }

        return list;
    }

    private static int CalculateScore(
        IEnumerable<AxeRule> violations, int passCount)
    {
        var penalty = violations.Sum(v => MapSeverity(v.Impact) switch
        {
            ViolationSeverity.Critical => v.Nodes.Count * 10,
            ViolationSeverity.Major => v.Nodes.Count * 5,
            ViolationSeverity.Moderate => v.Nodes.Count * 2,
            _ => v.Nodes.Count * 1
        });

        return Math.Max(0, 100 - penalty);
    }

    private static ViolationSeverity MapSeverity(string? impact) => impact switch
    {
        "critical" => ViolationSeverity.Critical,
        "serious" => ViolationSeverity.Major,
        "moderate" => ViolationSeverity.Moderate,
        _ => ViolationSeverity.Minor
    };

    private static string ExtractWcag(IEnumerable<string> tags) =>
        tags.FirstOrDefault(t => t.StartsWith("wcag") && t.Length > 6)
            ?.ToUpperInvariant()
            .Replace("WCAG", "WCAG ") ?? "WCAG 2.1";

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : string.Concat(s.AsSpan(0, max), "…");
}
