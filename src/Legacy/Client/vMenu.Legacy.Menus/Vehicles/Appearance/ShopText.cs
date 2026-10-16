using CitizenFX.Core.Native;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// The mod shop's own names, such as the wheel colours, live in a text block the game only loads inside the shop.
internal static class ShopText
{
    private const string Block = "MOD_MNU";

    private const int MenuTextSlot = 9;

    private static bool _owned;

    public static void Load()
    {
        if (_owned || API.HasAdditionalTextLoaded(MenuTextSlot))
        {
            return;
        }

        API.RequestAdditionalText(Block, MenuTextSlot);
        _owned = true;
    }

    public static void Unload()
    {
        if (!_owned)
        {
            return;
        }

        API.ClearAdditionalText(MenuTextSlot, false);
        _owned = false;
    }
}
