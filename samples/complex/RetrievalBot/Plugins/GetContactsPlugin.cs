// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Net.Http.Headers;
using Microsoft.Agents.Builder.App;
using Microsoft.SemanticKernel;

namespace RetrievalBot.Plugins;

/// <summary>
/// A Semantic Kernel plugin to retrieve contact information from Microsoft Graph.
/// </summary>
/// <param name="app">The parent <see cref="AgentApplication"/>.</param>
public class GetContactsPlugin(AgentApplication app)
{
    /// <summary>
    /// Retrieve the user's personal contacts.
    /// </summary>
    /// <returns>The user's contacts as a JSON string.</returns>
    [Description("This function talks to Microsoft Graph APIs and gets user contacts with their full name, email id and office location.")]
    [KernelFunction]
    public async Task<string> GetContactsAsync()
    {
        string accessToken = app.UserAuthorization.GetTurnToken("graph");
        string graphApiUrl = $"https://graph.microsoft.com/v1.0/me/contacts";

        HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.GetAsync(graphApiUrl);
        if (response.IsSuccessStatusCode)
        {
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string retrievedData = await reader.ReadToEndAsync();
            return retrievedData;
        }
        else
        {
            Console.WriteLine("API Call failed.");
            return response.StatusCode.ToString();
        }
    }
}
