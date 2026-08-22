using System.Runtime.InteropServices;

using CitizenFX.Base;

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

[StructLayout(LayoutKind.Sequential)]
public sealed class ShopPedOutfitBuffer : INativeStruct
{
#pragma warning disable CS0649
#pragma warning disable IDE0044
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
#pragma warning restore IDE0044
#pragma warning restore CS0649

    public uint LockHash => (uint)_lockHash;

    public uint NameHash => (uint)_nameHash;

    public int Props => (int)_props;

    public int Components => (int)_components;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);
}

[StructLayout(LayoutKind.Sequential)]
public sealed class ShopPedComponentBuffer : INativeStruct
{
#pragma warning disable CS0649
#pragma warning disable IDE0044
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
#pragma warning restore IDE0044
#pragma warning restore CS0649

    public uint LockHash => (uint)_lockHash;

    public int Drawable => (int)_drawable;

    public int Texture => (int)_texture;

    public int Slot => (int)_componentType;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);
}

[StructLayout(LayoutKind.Sequential)]
public sealed class ShopPedPropBuffer : INativeStruct
{
#pragma warning disable CS0649
#pragma warning disable IDE0044
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
#pragma warning restore IDE0044
#pragma warning restore CS0649

    public uint LockHash => (uint)_lockHash;

    public int Drawable => (int)_propIndex;

    public int Texture => (int)_texture;

    public int Slot => (int)_anchor;

    public string Label => ScriptLabel.Read(_label0, _label1, _label2, _label3);
}

[StructLayout(LayoutKind.Sequential)]
public sealed class OutfitVariantBuffer : INativeStruct
{
#pragma warning disable CS0649
#pragma warning disable IDE0044
    private ulong _nameHash;
    private ulong _enumValue;
    private ulong _type;
#pragma warning restore IDE0044
#pragma warning restore CS0649

    public uint NameHash => (uint)_nameHash;

    public int EnumValue => (int)_enumValue;

    public int Slot => (int)_type;
}
