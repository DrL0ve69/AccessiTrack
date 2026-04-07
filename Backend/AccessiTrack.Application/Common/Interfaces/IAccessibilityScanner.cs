using AccessiTrack.Application.Violations.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Common.Interfaces;

/// <summary>
/// Contract for the automatic WCAG accessibility scanner.
/// Implementations fetch a URL and return a list of detected violations.
/// </summary>
public interface IAccessibilityScanner
{
    /// <summary>
    /// Scans the given URL for WCAG accessibility violations.
    /// </summary>
    Task<IEnumerable<LogViolationCommand>> ScanUrlAsync(string url, CancellationToken ct = default);
}
