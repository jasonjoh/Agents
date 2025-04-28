// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.Agents.Builder.App;
using Microsoft.Graph;
using Microsoft.Graph.Me.SendMail;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.SemanticKernel;

namespace RetrievalBot.Plugins;

/// <summary>
/// A Semantic Kernel plugin to send email using Microsoft Graph.
/// </summary>
/// <param name="app">The parent <see cref="AgentApplication"/>.</param>
public class SendMailPlugin(AgentApplication app)
{
    /// <summary>
    /// Send an email from the user.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <returns>A success message if the email is sent successfully; otherwise, an error message.</returns>
    [Description("This function talks to Microsoft Graph SendMail API and sends mail to a given user email with a subject and a body text. It then returns success message to the user.")]
    [KernelFunction]
    public async Task<string> SendMailAsync(string email, string subject, string body)
    {
        string accessToken = app.UserAuthorization.GetTurnToken("graph");
        var tokenProvider = new StaticTokenProvider(accessToken);
        var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
        var graphClient = new GraphServiceClient(new HttpClientRequestAdapter(authProvider));

        var requestBody = new SendMailPostRequestBody
        {
            Message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = body,
                },
                ToRecipients =
                [
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = email,
                        },
                    },
                ],
            },
            SaveToSentItems = true,
        };

        try
        {
            await graphClient.Me.SendMail.PostAsync(requestBody);
            return "Mail sent successfully!";
        }
        catch (Exception ex)
        {
            return $"Mail sending failed: {ex.Message}";
        }
    }
}
