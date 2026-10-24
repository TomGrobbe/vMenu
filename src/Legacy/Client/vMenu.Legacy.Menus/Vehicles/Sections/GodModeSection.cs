using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// Unlike the other sections these rows are stored preferences rather than facts about a vehicle, so
// nothing here refills when the player changes what they are driving.
internal static class GodModeSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodInvincible,
            static () => VehicleGodMode.Invincible,
            VehicleGodMode.SetInvincible));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodEngine,
            static () => VehicleGodMode.ProtectEngine,
            VehicleGodMode.SetProtectEngine));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodVisual,
            static () => VehicleGodMode.PreventVisualDamage,
            VehicleGodMode.SetPreventVisualDamage));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodStrongWheels,
            static () => VehicleGodMode.StrongWheels,
            VehicleGodMode.SetStrongWheels));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodBulletproofTyres,
            static () => VehicleGodMode.BulletproofTyres,
            VehicleGodMode.SetBulletproofTyres));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodRamp,
            static () => VehicleGodMode.PreventRampDamage,
            VehicleGodMode.SetPreventRampDamage));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodAutoRepair,
            static () => VehicleGodMode.AutoRepair,
            VehicleGodMode.SetAutoRepair));
    }

    // No gate on the rows: the entry that opens this menu carries the one permission covering the lot.
    private static CheckboxEntry Row(string textKey, Func<bool> read, Action<bool> write) =>
        new(textKey)
        {
            ReadState = read,
            OnChanged = changed => write(changed.Checked),
        };
}
