// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace RetrievalBot.Plugins;

/// <summary>
/// Semantic Kernel plugins for date and time.
/// </summary>
public class DateTimePlugin
{
    /// <summary>
    /// Get the current date.
    /// </summary>
    /// <example>
    /// {{time.date}} => Sunday, 12 January, 2031.
    /// </example>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The current date.</returns>
    [KernelFunction]
    [Description("Get the current date")]
    public string Date(IFormatProvider? formatProvider = null)
    {
        // Example: Sunday, 12 January, 2025
        var date = DateTimeOffset.Now.ToString("D", formatProvider);
        return date;
    }

    /// <summary>
    /// Get the current date.
    /// </summary>
    /// <example>
    /// {{time.today}} => Sunday, 12 January, 2031.
    /// </example>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The current date.</returns>
    [KernelFunction]
    [Description("Get the current date")]
    public string Today(IFormatProvider? formatProvider = null)
    {
        // Example: Sunday, 12 January, 2025
        return Date(formatProvider);
    }

    /// <summary>
    /// Get the current date and time in the local time zone.
    /// </summary>
    /// <example>
    /// {{time.now}} => Sunday, January 12, 2025 9:15 PM.
    /// </example>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The current date and time in the local time zone.</returns>
    [KernelFunction]
    [Description("Get the current date and time in the local time zone")]
    public string Now(IFormatProvider? formatProvider = null)
    {
        // Sunday, January 12, 2025 9:15 PM
        return DateTimeOffset.Now.ToString("f", formatProvider);
    }

    /// <summary>
    /// Gets the number of days until Microsoft Build 2025.
    /// </summary>
    /// <returns>The number of days until Microsoft Build 2025.</returns>
    [KernelFunction]
    [Description("Get the number of days to Microsoft Build 2025")]
    public double DaysToBuild()
    {
        DateTime d1 = DateTime.Now;

        // Build 2025 starts on May 19th 2025
        DateTime d2 = DateTime.Parse("5/19/2025 12:00:01 AM");

        TimeSpan difference = d2 - d1;
        var days = difference.TotalDays;
        return days;
    }
}
