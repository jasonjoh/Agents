// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RetrievalBot.Agents;

/// <summary>
/// Indicates the type of content in the response.
/// </summary>
public enum RetrievalAgentResponseContentType
{
    /// <summary>
    /// Plain text content.
    /// </summary>
    [JsonPropertyName("text")]
    Text,

    /// <summary>
    /// Adaptive Card content.
    /// </summary>
    [JsonPropertyName("adaptive-card")]
    AdaptiveCard,
}

/// <summary>
/// Represents a response from the retrieval agent.
/// </summary>
public class RetrievalAgentResponse
{
    /// <summary>
    /// Gets or sets the content type of the response.
    /// </summary>
    [JsonPropertyName("contentType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RetrievalAgentResponseContentType ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content of the response.
    /// </summary>
    [JsonPropertyName("content")]
    [Description("The content of the response, may be plain text, or JSON based adaptive card but must be a string.")]
    public string Content { get; set; } = string.Empty;
}
