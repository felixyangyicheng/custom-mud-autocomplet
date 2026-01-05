using CustomAutoComplet.Data;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Services.Implementations;
using Microsoft.AspNetCore.SignalR;

namespace CustomAutoComplet.Hubs;



public class UserHub : Hub {
    public async Task RefreshEmployees(List<User> employees)
    {

        await Clients.All.SendAsync("RefreshEmployees", employees);
    }
}
