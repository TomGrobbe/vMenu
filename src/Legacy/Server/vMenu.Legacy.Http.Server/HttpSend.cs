using vMenu.Enhanced.Http.Server.Bridge;

namespace vMenu.Enhanced.Http.Server;

public static class HttpSend
{
    public static async Task<HttpReply> SendAsync(HttpRequest request)
    {
        var slot = new HttpSlot();

        NetBridge.Send(request, slot);

        return await HttpWait.ForAsync(slot, request.TimeoutMs);
    }
}
