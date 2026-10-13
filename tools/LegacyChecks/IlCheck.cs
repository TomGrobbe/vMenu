using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace vMenu.LegacyChecks;

internal static class IlCheck
{
    public static int Run(IEnumerable<string> assemblies)
    {
        var problems = 0;
        var paths = assemblies.ToList();
        var readonlyFields = ReadonlyFields(paths);

        foreach (var path in paths)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();

            foreach (var attributeHandle in reader.CustomAttributes)
            {
                if (reader.GetCustomAttribute(attributeHandle).Parent.Kind == HandleKind.GenericParameterConstraint)
                {
                    Console.Error.WriteLine($"{Path.GetFileName(path)}: has an attribute on a generic constraint, which Mono refuses to load");
                    problems++;
                }
            }

            foreach (var typeHandle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(typeHandle);

                if (IsStruct(reader, type))
                {
                    var finder = new SelfFinder(typeHandle);

                    foreach (var fieldHandle in type.GetFields())
                    {
                        var field = reader.GetFieldDefinition(fieldHandle);

                        if (field.DecodeSignature(finder, null).SelfInGeneric)
                        {
                            Console.Error.WriteLine($"{Path.GetFileName(path)}: {Name(reader, typeHandle)}::{reader.GetString(field.Name)}: holds a generic struct of its own struct");
                            problems++;
                        }
                    }
                }

                foreach (var methodHandle in type.GetMethods())
                {
                    var method = reader.GetMethodDefinition(methodHandle);
                    var name = $"{Name(reader, typeHandle)}::{reader.GetString(method.Name)}";

                    foreach (var problem in Check(reader, pe, typeHandle, method, readonlyFields))
                    {
                        Console.Error.WriteLine($"{Path.GetFileName(path)}: {name}: {problem}");
                        problems++;
                    }
                }
            }
        }

        return problems;
    }

    private static IEnumerable<string> Check(MetadataReader reader, PEReader pe, TypeDefinitionHandle declaring, MethodDefinition method, Dictionary<string, bool> readonlyFields)
    {
        var signature = reader.GetBlobReader(method.Signature);
        var header = signature.ReadSignatureHeader();

        if (header.IsGeneric)
        {
            signature.ReadCompressedInteger();
        }

        signature.ReadCompressedInteger();

        if (signature.ReadSignatureTypeCode() == SignatureTypeCode.ByReference)
        {
            yield return "returns by reference";
        }

        if (method.RelativeVirtualAddress == 0)
        {
            yield break;
        }

        var body = pe.GetMethodBody(method.RelativeVirtualAddress);
        var il = body.GetILReader();
        var methodName = reader.GetString(method.Name);
        var isConstructor = methodName is ".ctor";
        var isTypeConstructor = methodName is ".cctor";

        while (il.RemainingBytes > 0)
        {
            int code = il.ReadByte();

            if (code == 0xFE)
            {
                code = 0xFE00 | il.ReadByte();
            }

            switch (code)
            {
                case 0x7C or 0x7F:
                    var handle = MetadataTokens.EntityHandle(il.ReadInt32());

                    if (FieldName(reader, handle) is { } field && readonlyFields.TryGetValue(field, out var isStatic))
                    {
                        var ownCtor = field.StartsWith(Name(reader, declaring) + "::", StringComparison.Ordinal) && (isStatic ? isTypeConstructor : isConstructor);

                        if (!ownCtor)
                        {
                            yield return $"takes the address of readonly field {field}";
                        }
                    }

                    continue;

                case 0xFE1E:
                    yield return "uses the readonly. prefix";
                    continue;

                case 0xFE0F:
                    yield return "uses stackalloc";
                    continue;
            }

            Skip(ref il, code);
        }

        foreach (var problem in BoxedGenericJoins(reader, body))
        {
            yield return problem;
        }
    }

    private static IEnumerable<string> BoxedGenericJoins(MetadataReader reader, MethodBodyBlock body)
    {
        var instructions = new List<(int Offset, int Code, int Token)>();
        var targets = new HashSet<int>();
        var il = body.GetILReader();

        while (il.RemainingBytes > 0)
        {
            var offset = il.Offset;
            int code = il.ReadByte();

            if (code == 0xFE)
            {
                code = 0xFE00 | il.ReadByte();
            }

            var token = 0;

            switch (code)
            {
                case >= 0x2B and <= 0x37 or 0xDE:
                    var shortDelta = il.ReadSByte();
                    targets.Add(il.Offset + shortDelta);
                    break;
                case >= 0x38 and <= 0x44 or 0xDD:
                    var delta = il.ReadInt32();
                    targets.Add(il.Offset + delta);
                    break;
                case 0x45:
                    var count = il.ReadInt32();
                    var start = il.Offset + count * 4;

                    for (var i = 0; i < count; i++)
                    {
                        targets.Add(start + il.ReadInt32());
                    }

                    break;
                case 0x8C:
                    token = il.ReadInt32();
                    break;
                default:
                    Skip(ref il, code);
                    break;
            }

            instructions.Add((offset, code, token));
        }

        for (var i = 0; i + 1 < instructions.Count; i++)
        {
            if (instructions[i].Code == 0x8C
                && IsGenericParameter(reader, instructions[i].Token)
                && (instructions[i + 1].Code is 0x2B or 0x38 || targets.Contains(instructions[i + 1].Offset)))
            {
                yield return $"carries a boxed generic value into a branch join at IL_{instructions[i].Offset:x4}";
            }
        }
    }

    private static bool IsGenericParameter(MetadataReader reader, int token)
    {
        var handle = MetadataTokens.EntityHandle(token);

        if (handle.Kind != HandleKind.TypeSpecification)
        {
            return false;
        }

        var blob = reader.GetBlobReader(reader.GetTypeSpecification((TypeSpecificationHandle)handle).Signature);
        var kind = blob.ReadSignatureTypeCode();

        return kind is SignatureTypeCode.GenericTypeParameter or SignatureTypeCode.GenericMethodParameter;
    }

    private static void Skip(ref BlobReader il, int code)
    {
        switch (code)
        {
            case 0x45:
                var count = il.ReadInt32();
                il.Offset += count * 4;
                return;
            case 0x20 or 0x22 or 0x28 or 0x29 or 0x6F or 0x70 or 0x71 or 0x72 or 0x73 or 0x74 or 0x75 or 0x79 or 0x7B or 0x7C or 0x7D or 0x7E or 0x7F or 0x80
                or 0x81 or 0x8C or 0x8D or 0x8F or 0xA3 or 0xA4 or 0xA5 or 0xC2 or 0xC6 or 0xD0 or 0xDD or 0x38 or 0x39 or 0x3A
                or 0x3B or 0x3C or 0x3D or 0x3E or 0x3F or 0x40 or 0x41 or 0x42 or 0x43 or 0x44 or 0x27
                or 0xFE06 or 0xFE07 or 0xFE15 or 0xFE16 or 0xFE1C:
                il.Offset += 4;
                return;
            case 0x21 or 0x23:
                il.Offset += 8;
                return;
            case 0x0E or 0x0F or 0x10 or 0x11 or 0x12 or 0x13 or 0x1F or 0x2B or 0x2C or 0x2D or 0x2E or 0x2F or 0x30 or 0x31
                or 0x32 or 0x33 or 0x34 or 0x35 or 0x36 or 0x37 or 0xDE or 0xFE12 or 0xFE19:
                il.Offset += 1;
                return;
            case 0xFE09 or 0xFE0A or 0xFE0B or 0xFE0C or 0xFE0D or 0xFE0E:
                il.Offset += 2;
                return;
        }
    }

    private static Dictionary<string, bool> ReadonlyFields(List<string> paths)
    {
        var fields = new Dictionary<string, bool>(StringComparer.Ordinal);

        foreach (var path in paths)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();

            foreach (var fieldHandle in reader.FieldDefinitions)
            {
                var field = reader.GetFieldDefinition(fieldHandle);

                if ((field.Attributes & FieldAttributes.InitOnly) != 0)
                {
                    fields[$"{Name(reader, field.GetDeclaringType())}::{reader.GetString(field.Name)}"] = (field.Attributes & FieldAttributes.Static) != 0;
                }
            }
        }

        return fields;
    }

    private static string? FieldName(MetadataReader reader, EntityHandle handle)
    {
        switch (handle.Kind)
        {
            case HandleKind.FieldDefinition:
                var definition = reader.GetFieldDefinition((FieldDefinitionHandle)handle);
                return $"{Name(reader, definition.GetDeclaringType())}::{reader.GetString(definition.Name)}";
            case HandleKind.MemberReference:
                var reference = reader.GetMemberReference((MemberReferenceHandle)handle);
                return TypeName(reader, reference.Parent) is { } owner ? $"{owner}::{reader.GetString(reference.Name)}" : null;
            default:
                return null;
        }
    }

    private static string? TypeName(MetadataReader reader, EntityHandle handle)
    {
        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition:
                return Name(reader, (TypeDefinitionHandle)handle);
            case HandleKind.TypeReference:
                var reference = reader.GetTypeReference((TypeReferenceHandle)handle);
                var name = reader.GetString(reference.Name);

                return reference.ResolutionScope.Kind == HandleKind.TypeReference
                    ? $"{TypeName(reader, (EntityHandle)reference.ResolutionScope)}+{name}"
                    : $"{reader.GetString(reference.Namespace)}.{name}";
            case HandleKind.TypeSpecification:
                var blob = reader.GetBlobReader(reader.GetTypeSpecification((TypeSpecificationHandle)handle).Signature);

                if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance)
                {
                    return null;
                }

                blob.ReadByte();
                return TypeName(reader, blob.ReadTypeHandle());
            default:
                return null;
        }
    }

    private static bool IsStruct(MetadataReader reader, TypeDefinition type)
    {
        if (type.BaseType.Kind != HandleKind.TypeReference)
        {
            return false;
        }

        var baseType = reader.GetTypeReference((TypeReferenceHandle)type.BaseType);
        return reader.GetString(baseType.Namespace) == "System" && reader.GetString(baseType.Name) == "ValueType";
    }

    private readonly record struct Shape(bool HasSelf, bool SelfInGeneric, bool IsValueType = false);

    private sealed class SelfFinder(TypeDefinitionHandle self) : ISignatureTypeProvider<Shape, object?>
    {
        public Shape GetArrayType(Shape elementType, ArrayShape shape) => elementType;

        public Shape GetByReferenceType(Shape elementType) => elementType;

        public Shape GetFunctionPointerType(MethodSignature<Shape> signature) => default;

        public Shape GetGenericInstantiation(Shape genericType, System.Collections.Immutable.ImmutableArray<Shape> typeArguments) =>
            new(genericType.HasSelf || typeArguments.Any(static argument => argument.HasSelf),
                typeArguments.Any(static argument => argument.SelfInGeneric) || (genericType.IsValueType && typeArguments.Any(static argument => argument.HasSelf)),
                genericType.IsValueType);

        public Shape GetGenericMethodParameter(object? genericContext, int index) => default;

        public Shape GetGenericTypeParameter(object? genericContext, int index) => default;

        public Shape GetModifiedType(Shape modifier, Shape unmodifiedType, bool isRequired) => unmodifiedType;

        public Shape GetPinnedType(Shape elementType) => elementType;

        public Shape GetPointerType(Shape elementType) => elementType;

        public Shape GetPrimitiveType(PrimitiveTypeCode typeCode) => default;

        public Shape GetSZArrayType(Shape elementType) => elementType;

        public Shape GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) =>
            new(handle == self, false, rawTypeKind == (byte)SignatureTypeKind.ValueType);

        public Shape GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) =>
            new(false, false, rawTypeKind == (byte)SignatureTypeKind.ValueType);

        public Shape GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }

    private static string Name(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        var name = reader.GetString(type.Name);
        var declaring = type.GetDeclaringType();

        return declaring.IsNil
            ? $"{reader.GetString(type.Namespace)}.{name}"
            : $"{Name(reader, declaring)}+{name}";
    }
}
