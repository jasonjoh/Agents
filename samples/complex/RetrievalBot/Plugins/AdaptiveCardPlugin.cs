// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace RetrievalBot.Plugins;

/// <summary>
/// A Semantic Kernel plugin to generate Adaptive Cards based on provided data.
/// </summary>
public class AdaptiveCardPlugin
{
    private const string Instructions = """
        When given data, please generate an adaptive card that displays the information in
        a visually appealing way. Make sure to only return the valid adaptive card
        JSON string in the response.
        """;

    /// <summary>
    /// Generates an Adaptive Card based on the provided data.
    /// </summary>
    /// <param name="kernel">The <see cref="Kernel"/>.</param>
    /// <param name="data">The data to use to generate and Adaptive Card.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A JSON string containing an Adaptive Card payload.</returns>
    [KernelFunction]
    public async Task<string> GetAdaptiveCardForDataAsync(Kernel kernel, string data, CancellationToken cancellationToken)
    {
        // Create a chat history with the instructions as a system message and the data as a user message
        ChatHistory chat = new(Instructions);
        chat.AddUserMessage(data);

        // Invoke the model to get a response
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatCompletion.GetChatMessageContentAsync(chat, cancellationToken: cancellationToken);

        return response.ToString();
    }
}
