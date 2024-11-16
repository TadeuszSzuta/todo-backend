using Microsoft.AspNetCore.SignalR;

namespace ToDoApi
{
    public class TodoHub:Hub
    {

        public async Task NotifyTodosUpdated()
        {
            await Clients.All.SendAsync("TodosUpdated");
        }
    }
}
