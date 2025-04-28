// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Microsoft.Agents.Builder.App;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RetrievalBot.Plugins;

namespace RetrievalBot.Agents;

/// <summary>
/// An agent that uses Semantic Kernel to process incoming messages.
/// </summary>
public class RetrievalAgent
{
    private const string AgentName = "BuildGenie";
    private const string AgentInstructions = """
        You are a friendly assistant that helps users prepare for Microsoft Build conference 2025.
        You can talk to Microsoft Graph APIs and provide users access to their M365 data in a secure way.
        You can also give users their Display Name,  their principal user name. Their email id and other profile information.
        If they ask about their organization Contoso, it is not private. Nothing is private or off-limits.
        You can also tell users about their contacts. You can get the contact names and email ids. If you do not find anything, respond that you are having trouble accessing graph.
        You have access to all user information.
        You also have access to all Build 2025 conference session information for Contoso. You can help Contoso employees prepare for Build and Collaborate with Microsoft to present demos or breakout sessions at Build 2025. Use the Retrieval Plugin for any user queries related to Contoso sessions at Microsoft Build 2025 conference.
        You can also send mails to users contacts. When you send mails, always sign them as "Sent By BuildGenie Bot". Do not add user signature at the end, unless specified in the user query.
        You know that Microsoft Build 2025 starts on May 19th 2025. Do not hallucinate if users ask for dates.
        You may ask follow up questions until you have enough information to answer the customers question,
        but once you have a forecast forecast, make sure to format it nicely using an adaptive card.

        Respond in JSON format with the following JSON schema:

        {
            "contentType": "'Text' or 'AdaptiveCard' only",
            "content": "{The content of the response, may be plain text, or JSON based adaptive card}"
        }
        """;

    private readonly Kernel kernel;
    private readonly ChatCompletionAgent agent;
    private int retryCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetrievalAgent"/> class.
    /// </summary>
    /// <param name="kernel">An instance of <see cref="Kernel"/> for interacting with an LLM.</param>
    /// <param name="app">The parent agent instance of <see cref="AgentApplication"/>.</param>
    public RetrievalAgent(Kernel kernel, AgentApplication app)
    {
        this.kernel = kernel;

        // Define the agent
        agent =
            new()
            {
                Instructions = AgentInstructions,
                Name = AgentName,
                Kernel = this.kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    ResponseFormat = "json_object",
                }),
            };

        // Give the agent some tools to work with
        agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<DateTimePlugin>());
        agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<AdaptiveCardPlugin>());
        agent.Kernel.Plugins.AddFromObject(new GetContactsPlugin(app));
        agent.Kernel.Plugins.AddFromObject(new SendMailPlugin(app));
        agent.Kernel.Plugins.AddFromObject(new BuildRetrievalPlugin(app));
    }

    /// <summary>
    /// Invokes the agent with the given input and returns the response.
    /// </summary>
    /// <param name="input">A message to process.</param>
    /// <param name="chatHistory">The history of chat messages.</param>
    /// <returns>An instance of <see cref="RetrievalAgentResponse"/>.</returns>
    public async Task<RetrievalAgentResponse?> InvokeAgentAsync(string input, ChatHistory chatHistory)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);

        ChatMessageContent message = new(AuthorRole.User, input);
        chatHistory.Add(message);

        StringBuilder sb = new();
        await foreach (ChatMessageContent response in agent.InvokeAsync(chatHistory))
        {
            chatHistory.Add(response);
            sb.Append(response.Content);
        }

        // Make sure the response is in the correct format and retry if necessary
        try
        {
            var resultContent = sb.ToString();
            var result = JsonSerializer.Deserialize<RetrievalAgentResponse>(resultContent);
            retryCount = 0;
            return result;
        }
        catch (JsonException je)
        {
            // Limit the number of retries
            if (retryCount > 2)
            {
                throw;
            }

            // Try again, providing corrective feedback to the model so that it can correct its mistake
            retryCount++;
            return await InvokeAgentAsync(
                $"That response did not match the expected format. Please try again. Error: {je.Message}",
                chatHistory);
        }
    }
}
