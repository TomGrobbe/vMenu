namespace vMenu.Enhanced.BrokenNatives;

public sealed class PedHeadBlendBuffer
{
    private ulong _firstShape;
    private ulong _secondShape;
    private ulong _thirdShape;
    private ulong _firstSkin;
    private ulong _secondSkin;
    private ulong _thirdSkin;
    private ulong _shapeMix;
    private ulong _skinMix;
    private ulong _thirdMix;
    private ulong _isParent;

    public int FirstShape => (int)_firstShape;

    public int SecondShape => (int)_secondShape;

    public int ThirdShape => (int)_thirdShape;

    public int FirstSkin => (int)_firstSkin;

    public int SecondSkin => (int)_secondSkin;

    public int ThirdSkin => (int)_thirdSkin;

    public float ShapeMix => Float(_shapeMix);

    public float SkinMix => Float(_skinMix);

    public float ThirdMix => Float(_thirdMix);

    public bool IsParent => _isParent != 0;

    public bool IsEmpty =>
        _firstShape == 0 && _secondShape == 0 && _thirdShape == 0
        && _firstSkin == 0 && _secondSkin == 0 && _thirdSkin == 0
        && _shapeMix == 0 && _skinMix == 0 && _thirdMix == 0;

    private static float Float(ulong slot) => BitConverter.Int32BitsToSingle((int)slot);

    public const int Slots = 10;

    internal void Load(ulong[] slots)
    {
        _firstShape = slots[0];
        _secondShape = slots[1];
        _thirdShape = slots[2];
        _firstSkin = slots[3];
        _secondSkin = slots[4];
        _thirdSkin = slots[5];
        _shapeMix = slots[6];
        _skinMix = slots[7];
        _thirdMix = slots[8];
        _isParent = slots[9];
    }
}
