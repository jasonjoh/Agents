// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RetrievalBot.Controllers;

/// <summary>
/// ASP.Net Controller that receives incoming HTTP requests from the Azure Bot Service or
/// other configured event activity protocol sources.
/// </summary>
/// <param name="adapter">The adapter interface to send incoming messages to the agent.</param>
/// <param name="agent">The agent to use to process the message.</param>
[Authorize]
[ApiController]
[Route("api/messages")]
public class BotController(IAgentHttpAdapter adapter, IAgent agent) : ControllerBase
{
    /// <summary>
    /// Receives incoming messages.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> indicating the status of the asynchronous operation.</returns>
    [HttpPost]
    public Task PostAsync(CancellationToken cancellationToken)
        => adapter.ProcessAsync(Request, Response, agent, cancellationToken);
}
