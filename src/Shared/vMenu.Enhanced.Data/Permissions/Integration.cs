namespace vMenu.Enhanced.Data.Permissions;

[PermissionCategory]
[StaffOnly]
public static class Integration
{
    public const string All = "vMenu.Enhanced.Integration.All";

    public const string AllowlistBypass = "vMenu.Enhanced.Integration.Allowlist.Bypass";

    public const string QueueBypass = "vMenu.Enhanced.Integration.Queue.Bypass";
}
