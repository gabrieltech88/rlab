using Microsoft.AspNetCore.SignalR;
using RLab.Enums;

namespace RLab.Infrastructure.Orchestrator;
public class OrchestratorHub : Hub
{
    public async Task SendMessageAsync(State state, string message)
    {
        await Clients.All.SendAsync("ReceiveLog", state, message);
    }
}