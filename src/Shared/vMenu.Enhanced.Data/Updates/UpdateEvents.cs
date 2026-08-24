namespace vMenu.Enhanced.Data.Updates;

public static class UpdateEvents
{
    // Client to server, once it has its permissions. Answered only for staff.
    public const string Request = "vMenu.Enhanced:Updates:Request";

    // Server to client: the version, then the page to get it from.
    public const string Available = "vMenu.Enhanced:Updates:Available";
}
