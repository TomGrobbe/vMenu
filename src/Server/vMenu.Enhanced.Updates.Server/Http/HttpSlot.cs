namespace vMenu.Enhanced.Updates.Server.Http;

// Where a reply is left for the waiter to pick up. A late reply lands in a slot nobody is reading
// any more, which is exactly what should happen to one that arrived after the wait gave up.
internal sealed class HttpSlot
{
    public bool Done { get; private set; }

    public HttpReply? Reply { get; private set; }

    public void Complete(HttpReply reply)
    {
        if (Done)
        {
            return;
        }

        Reply = reply;
        Done = true;
    }
}
