namespace vMenu.Enhanced.Data.VehicleData;

public static class RemoteVehicleAction
{
    public const string Lock = "lock";

    public const string Engine = "engine";

    public const string Lights = "lights";

    public const string Door = "door";

    public const string AllDoors = "doors";

    public const string Window = "window";

    public const string AllWindows = "windows";

    public const string Explode = "explode";

    public const string On = "1";

    public const string Off = "0";

    public const string Toggle = "toggle";

    public const string Open = "open";

    public const string Shut = "shut";

    public const string Down = "down";

    public const string Up = "up";

    public const int DoorCount = 8;

    public const int WindowCount = 4;

    public const int LightsAutomatic = 0;

    public const int LightsOff = 1;

    public const int LightsOn = 3;

    public const int LockUnlocked = 1;

    public const int LockLocked = 2;

    public static bool IsLocked(int lockStatus) => lockStatus > LockUnlocked;
}
