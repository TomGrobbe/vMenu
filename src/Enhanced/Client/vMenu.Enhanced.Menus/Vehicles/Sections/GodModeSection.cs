using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// Unlike the other sections these rows are stored preferences rather than facts about a vehicle, so
// nothing here refills when the player changes what they are driving.
internal static class GodModeSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodInvincible,
            Loc.VehicleOptions.GodInvincibleDescription,
            static () => VehicleGodMode.Invincible,
            VehicleGodMode.SetInvincible));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodEngine,
            Loc.VehicleOptions.GodEngineDescription,
            static () => VehicleGodMode.ProtectEngine,
            VehicleGodMode.SetProtectEngine));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodVisual,
            Loc.VehicleOptions.GodVisualDescription,
            static () => VehicleGodMode.PreventVisualDamage,
            VehicleGodMode.SetPreventVisualDamage));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodStrongWheels,
            Loc.VehicleOptions.GodStrongWheelsDescription,
            static () => VehicleGodMode.StrongWheels,
            VehicleGodMode.SetStrongWheels));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodBulletproofTyres,
            Loc.VehicleOptions.GodBulletproofTyresDescription,
            static () => VehicleGodMode.BulletproofTyres,
            VehicleGodMode.SetBulletproofTyres));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodRamp,
            Loc.VehicleOptions.GodRampDescription,
            static () => VehicleGodMode.PreventRampDamage,
            VehicleGodMode.SetPreventRampDamage));

        menu.Entries.Add(Row(
            Loc.VehicleOptions.GodAutoRepair,
            Loc.VehicleOptions.GodAutoRepairDescription,
            static () => VehicleGodMode.AutoRepair,
            VehicleGodMode.SetAutoRepair));
    }

    // No gate on the rows: the entry that opens this menu carries the one permission covering the lot.
    private static CheckboxEntry Row(string textKey, string descriptionKey, Func<bool> read, Action<bool> write) =>
        new()
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            ReadState = read,
            OnChanged = changed => write(changed.Checked),
        };
}
