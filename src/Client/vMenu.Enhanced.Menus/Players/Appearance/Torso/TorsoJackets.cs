namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoJackets
{
    private static readonly int[] MaleJacketDrawables = [3, 4, 6, 7, 10];

    private static readonly int[] FemaleJacketDrawables = [1, 6, 7, 8, 10];

    internal static bool IsJacket(bool male, TorsoGarment top)
    {
        if (!top.IsDlc)
        {
            foreach (var drawable in male ? MaleJacketDrawables : FemaleJacketDrawables)
            {
                if (top.Drawable == drawable)
                {
                    return true;
                }
            }

            return false;
        }

        return top.Has(TorsoTags.Jacket);
    }
}
