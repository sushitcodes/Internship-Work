using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Form.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Connect automatically when login
    //public const string StaffAndAdminGroup = "StaffAndAdmin";
    //public override async Task OnConnectedAsync()
    //{
    //    if (Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true)
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, StaffAndAdminGroup);
    //    }
    //    await base.OnConnectedAsync();
    //}

    //// Discoonect when they close tab or they are logout
    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    if (Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true)
    //    {
    //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, StaffAndAdminGroup);

    //    }
    //    await base.OnDisconnectedAsync(exception);
    //}
}
