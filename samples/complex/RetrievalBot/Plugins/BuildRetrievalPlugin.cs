// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.M365Copilot.Beta;
using Microsoft.Agents.M365Copilot.Beta.Copilot.Retrieval;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.SemanticKernel;

namespace RetrievalBot.Plugins;

/// <summary>
/// A Semantic Kernel plugin to retrieve information from Microsoft Copilot Retrieval API.
/// </summary>
/// <param name="app">The parent <see cref="AgentApplication"/>.</param>
public class BuildRetrievalPlugin(AgentApplication app)
{
    /// <summary>
    /// Uses the Microsoft Copilot Retrieval API to query a SharePoint site's documents.
    /// </summary>
    /// <param name="userQuery">The user's query.</param>
    /// <returns>The results.</returns>
    [Description("This function uses Microsoft Copilot Retrieval API and gets Contoso Build session names, description, time slot, session type, and speakers nicely formatted. It will get all Contoso Microsoft collaborations at Build 2025 conference. It accepts user query as input and send out a chunk of relevant text and a link to the file in the results.")]
    [KernelFunction]
    public async Task<string> BuildRetrievalAsync(string userQuery)
    {
        string accessToken = app.UserAuthorization.GetTurnToken("graph");
        var tokenProvider = new StaticTokenProvider(accessToken);
        var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
        var apiClient = new BaseM365CopilotClient(new HttpClientRequestAdapter(authProvider));

        #pragma warning disable CS0618 // Type or member is obsolete
        var response = await apiClient.Copilot.Retrieval.PostAsync(new RetrievalPostRequestBody()
        {
            QueryString = userQuery,
            FilterExpression = "(path:\"https://m365cpi20322491.sharepoint.com/sites/CCSTest/Shared%20Documents/Build/\")",
            ResourceMetadata = [string.Empty],
            MaximumNumberOfResults = 1,
        });
        #pragma warning restore CS0618 // Type or member is obsolete

        return System.Text.Json.JsonSerializer.Serialize(response);
    }
}
