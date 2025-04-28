// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kiota.Abstractions.Authentication;

namespace RetrievalBot.Plugins;

/// <summary>
/// An implementation of <see cref="IAccessTokenProvider"/> that provides a static token for Microsoft Graph API calls.
/// </summary>
/// <param name="token">The static token.</param>
public class StaticTokenProvider(string token) : IAccessTokenProvider
{
    /// <inheritdoc/>
    public AllowedHostsValidator AllowedHostsValidator => new(["graph.microsoft.com"]);

    /// <inheritdoc/>
    public Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return AllowedHostsValidator.AllowedHosts.Contains(uri.Host) ? Task.FromResult(token) :
            Task.FromResult(string.Empty);
    }
}
