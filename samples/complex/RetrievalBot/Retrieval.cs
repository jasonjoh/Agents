// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
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
        var agentThread = new ChatHistoryAgentThread(chatHistory);

        RetrievalAgent retrievalAgent = new(kernel, this);

        // Invoke the RetrievalAgent to process the message
        var retrievalResponse = await retrievalAgent.InvokeAgentAsync(turnContext.Activity.Text, agentThread);
        if (retrievalResponse == null)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text("Sorry, I ran into a problem responding to your query. Please try again."),
                cancellationToken);
            return;
        }

        // Create a response message based on the response content type from the RetrievalAgent
        IActivity response = retrievalResponse.ContentType switch
        {
            RetrievalAgentResponseContentType.AdaptiveCard => MessageFactory.Attachment(new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = retrievalResponse.Content,
            }),
            _ => MessageFactory.Text(retrievalResponse.Content),
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
