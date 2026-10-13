namespace vMenu.Enhanced.BrokenNatives;

internal static class ScriptLabel
{
    private const int Length = 32;

    internal static string Read(ulong first, ulong second, ulong third, ulong fourth)
    {
        var slots = new[] { first, second, third, fourth };
        var characters = new char[Length];
        var used = 0;

        foreach (var slot in slots)
        {
            for (var offset = 0; offset < 8; offset++)
            {
                var value = (byte)(slot >> (offset * 8));

                if (value == 0)
                {
                    return new string(characters, 0, used);
                }

                characters[used++] = (char)value;
            }
        }

        return new string(characters, 0, used);
    }
}

public sealed class ShopPedOutfitBuffer
{
    private ulong _lockHash;
    private ulong _nameHash;
    private ulong _cost;
    private ulong _props;
    private ulong _components;
    private ulong _shop;
    private ulong _character;
    private ulong _label0;
    private ulong _label1;
    private ulong _label2;
    private ulong _label3;

    public uint LockHash => (uint)_lockHash;

    public uint NameHash => (uint)_nameHash;

    public int Props => (int)_props;

    public int Components => (int)_components;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);

    public const int Slots = 11;

    internal void Load(ulong[] slots)
    {
        _lockHash = slots[0];
        _nameHash = slots[1];
        _cost = slots[2];
        _props = slots[3];
        _components = slots[4];
        _shop = slots[5];
        _character = slots[6];
        _label0 = slots[7];
        _label1 = slots[8];
        _label2 = slots[9];
        _label3 = slots[10];
    }
}

public sealed class ShopPedComponentBuffer
{
    private ulong _lockHash;
    private ulong _nameHash;
    private ulong _locate;
    private ulong _drawable;
    private ulong _texture;
    private ulong _cost;
    private ulong _componentType;
    private ulong _shop;
    private ulong _character;
    private ulong _label0;
    private ulong _label1;
    private ulong _label2;
    private ulong _label3;

    public uint LockHash => (uint)_lockHash;

    public int Drawable => (int)_drawable;

    public int Texture => (int)_texture;

    public int Slot => (int)_componentType;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);

    public const int Slots = 13;

    internal void Load(ulong[] slots)
    {
        _lockHash = slots[0];
        _nameHash = slots[1];
        _locate = slots[2];
        _drawable = slots[3];
        _texture = slots[4];
        _cost = slots[5];
        _componentType = slots[6];
        _shop = slots[7];
        _character = slots[8];
        _label0 = slots[9];
        _label1 = slots[10];
        _label2 = slots[11];
        _label3 = slots[12];
    }
}

public sealed class ShopPedPropBuffer
{
    private ulong _lockHash;
    private ulong _nameHash;
    private ulong _locate;
    private ulong _propIndex;
    private ulong _texture;
    private ulong _cost;
    private ulong _anchor;
    private ulong _shop;
    private ulong _character;
    private ulong _label0;
    private ulong _label1;
    private ulong _label2;
    private ulong _label3;

    public uint LockHash => (uint)_lockHash;

    public int Drawable => (int)_propIndex;

    public int Texture => (int)_texture;

    public int Slot => (int)_anchor;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);

    public const int Slots = 13;

    internal void Load(ulong[] slots)
    {
        _lockHash = slots[0];
        _nameHash = slots[1];
        _locate = slots[2];
        _propIndex = slots[3];
        _texture = slots[4];
        _cost = slots[5];
        _anchor = slots[6];
        _shop = slots[7];
        _character = slots[8];
        _label0 = slots[9];
        _label1 = slots[10];
        _label2 = slots[11];
        _label3 = slots[12];
    }
}

public sealed class OutfitVariantBuffer
{
    private ulong _nameHash;
    private ulong _enumValue;
    private ulong _type;

    public uint NameHash => (uint)_nameHash;

    public int EnumValue => (int)_enumValue;

    public int Slot => (int)_type;

    public const int Slots = 3;

    internal void Load(ulong[] slots)
    {
        _nameHash = slots[0];
        _enumValue = slots[1];
        _type = slots[2];
    }
}
