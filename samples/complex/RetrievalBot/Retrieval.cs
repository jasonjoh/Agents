// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RetrievalBot.Agents;

namespace RetrievalBot;

/// <summary>
/// An agent that uses Semantic Kernel to process incoming messages.
/// </summary>
/// <param name="options">The <see cref="AgentApplicationOptions"/> provided by dependency injection.</param>
/// <param name="kernel">The <see cref="Kernel"/> instance used for processing.</param>
public class Retrieval(AgentApplicationOptions options, Kernel kernel) : AgentApplication(options)
{
    [Route(RouteType = RouteType.Activity, Type = ActivityTypes.Message, Rank = RouteRank.Last)]
    private async Task MessageActivityAsync(
        ITurnContext turnContext,
        ITurnState turnState,
        CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

        var chatHistory = turnState.GetValue("conversation.chatHistory", () => new ChatHistory());

        RetrievalAgent weatherAgent = new(kernel, this);

        // Invoke the RetrievalAgent to process the message
        var forecastResponse = await weatherAgent.InvokeAgentAsync(turnContext.Activity.Text, chatHistory);
        if (forecastResponse == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("Sorry, I couldn't get the weather forecast at the moment."),
                cancellationToken);
            return;
        }

        // Create a response message based on the response content type from the RetrievalAgent
        IActivity response = forecastResponse.ContentType switch
        {
            RetrievalAgentResponseContentType.AdaptiveCard => MessageFactory.Attachment(new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = forecastResponse.Content,
            }),
            _ => MessageFactory.Text(forecastResponse.Content),
        };

        // Send the response message back to the user.
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    [Route(RouteType = RouteType.Conversation, EventName = ConversationUpdateEvents.MembersAdded)]
    private async Task WelcomeMessageAsync(
        ITurnContext turnContext,
        ITurnState turnState,
        CancellationToken cancellationToken)
    {
        foreach (ChannelAccount member in turnContext.Activity.MembersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("Hello! I am Build Genie! I can help you prepare for Build Conference 2025!"),
                    cancellationToken);
            }
        }
    }
}
