using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace vMenu.LegacyChecks;

internal static class RefCheck
{
    public static readonly string[] OtherClientLibraries =
    [
        "Microsoft.CSharp", "Mono.CSharp", "Mono.Posix", "MsgPack", "System.Collections.Immutable", "System.Configuration", "System.Data",
        "System.Drawing", "System.EnterpriseServices", "System.Numerics", "System.Reflection.Metadata", "System.Runtime.Serialization",
        "System.ServiceModel.Internals", "System.Web", "System.Xml", "System.Xml.Linq",
    ];

    private static readonly string[] Accepted = ["MenuAPI.dll: System.Environment::get_CurrentManagedThreadId "];

    public static int Run(IEnumerable<string> libraries, IEnumerable<string> presentOnly, IReadOnlyList<string> assemblies)
    {
        var catalog = new Catalog();

        foreach (var library in libraries)
        {
            catalog.Add(library, library: true);
        }

        foreach (var name in presentOnly)
        {
            catalog.AddPresent(name);
        }

        foreach (var assembly in assemblies)
        {
            catalog.Add(assembly, library: false);
        }

        var problems = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var path in assemblies)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            var file = Path.GetFileName(path);
            var names = new NameProvider(reader);

            foreach (var handle in reader.AssemblyReferences)
            {
                var name = reader.GetString(reader.GetAssemblyReference(handle).Name);

                if (!catalog.HasAssembly(name))
                {
                    problems.Add($"{file}: references {name}, which FiveM does not have");
                }
            }

            foreach (var handle in reader.TypeReferences)
            {
                if (reader.GetTypeReference(handle).ResolutionScope.Kind != HandleKind.AssemblyReference)
                {
                    continue;
                }

                var type = names.TypeReferenceName(handle);

                if (type.EndsWith("Attribute", StringComparison.Ordinal) || !catalog.HasAssembly(AssemblyOf(reader, handle)) || catalog.IsPresentOnly(AssemblyOf(reader, handle)))
                {
                    continue;
                }

                if (!catalog.HasType(type))
                {
                    problems.Add($"{file}: type {type} does not exist in FiveM");
                }
                else if (catalog.IsHidden(type))
                {
                    problems.Add($"{file}: type {type} is not public in FiveM");
                }
            }

            foreach (var handle in reader.MemberReferences)
            {
                var member = reader.GetMemberReference(handle);
                var owner = names.OwnerName(member.Parent);

                if (owner is null || !catalog.HasType(owner))
                {
                    continue;
                }

                var key = member.GetKind() == MemberReferenceKind.Method
                    ? MethodKey(reader.GetString(member.Name), member.DecodeMethodSignature(names, null))
                    : reader.GetString(member.Name) + "|" + member.DecodeFieldSignature(names, null);

                switch (catalog.FindMember(owner, key))
                {
                    case Access.Missing:
                        problems.Add($"{file}: {owner}::{key.Replace("|", " ")} does not exist in FiveM");
                        break;
                    case Access.Hidden:
                        problems.Add($"{file}: {owner}::{key.Replace("|", " ")} is not public in FiveM");
                        break;
                    case Access.Critical:
                        problems.Add($"{file}: {owner}::{key.Replace("|", " ")} is security critical, which sandboxed code may not call");
                        break;
                }
            }
        }

        problems.RemoveWhere(problem => Accepted.Any(accepted => problem.StartsWith(accepted, StringComparison.Ordinal)));

        foreach (var problem in problems)
        {
            Console.Error.WriteLine(problem);
        }

        return problems.Count;
    }

    private static string AssemblyOf(MetadataReader reader, TypeReferenceHandle handle) =>
        reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)reader.GetTypeReference(handle).ResolutionScope).Name);

    private static string MethodKey(string name, MethodSignature<string> signature) =>
        $"{name}|`{signature.GenericParameterCount}({string.Join(",", signature.ParameterTypes)}){signature.ReturnType}";

    private enum Access
    {
        Public,
        Hidden,
        Critical,
        Missing,
    }

    private sealed class Catalog
    {
        private readonly HashSet<string> _assemblies = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _presentOnly = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Platform = new(["mscorlib", "System", "System.Core", "CitizenFX.Core", "CitizenFX.Core.Client"], StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, Dictionary<string, Access>> _members = new(StringComparer.Ordinal);
        private readonly HashSet<string> _hiddenTypes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string?> _bases = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _forwards = new(StringComparer.Ordinal);

        public void Add(string path, bool library)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            var names = new NameProvider(reader);

            var assemblyName = reader.GetString(reader.GetAssemblyDefinition().Name);
            var platform = library && Platform.Contains(assemblyName);
            _assemblies.Add(assemblyName);

            foreach (var handle in reader.ExportedTypes)
            {
                var exported = reader.GetExportedType(handle);
                var name = Join(reader.GetString(exported.Namespace), reader.GetString(exported.Name));
                _forwards[name] = name;
            }

            foreach (var handle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(handle);
                var name = names.TypeDefinitionName(handle);
                var members = new Dictionary<string, Access>(StringComparer.Ordinal);
                var typeLevel = platform ? SecurityLevel(reader, type.GetCustomAttributes(), type.GetDeclaringType().IsNil ? null : reader.GetTypeDefinition(type.GetDeclaringType())) : Access.Public;

                foreach (var methodHandle in type.GetMethods())
                {
                    var method = reader.GetMethodDefinition(methodHandle);
                    var visible = !library || (method.Attributes & MethodAttributes.MemberAccessMask) is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;
                    var level = platform ? SecurityLevel(reader, method.GetCustomAttributes(), null) ?? typeLevel : Access.Public;
                    members[MethodKey(reader.GetString(method.Name), method.DecodeSignature(names, null))] = !visible ? Access.Hidden : level ?? Access.Public;
                }

                foreach (var fieldHandle in type.GetFields())
                {
                    var field = reader.GetFieldDefinition(fieldHandle);
                    var visible = !library || (field.Attributes & FieldAttributes.FieldAccessMask) is FieldAttributes.Public or FieldAttributes.Family or FieldAttributes.FamORAssem;
                    var level = platform ? SecurityLevel(reader, field.GetCustomAttributes(), null) ?? typeLevel : Access.Public;
                    members[reader.GetString(field.Name) + "|" + field.DecodeSignature(names, null)] = !visible ? Access.Hidden : level ?? Access.Public;
                }

                _members[name] = members;

                if (library && !IsVisible(reader, type))
                {
                    _hiddenTypes.Add(name);
                }
                else
                {
                    _hiddenTypes.Remove(name);
                }

                _bases[name] = type.BaseType.IsNil ? null : names.OwnerName(type.BaseType);
            }
        }

        public void AddPresent(string name)
        {
            _assemblies.Add(name);
            _presentOnly.Add(name);
        }

        public bool IsPresentOnly(string name) => _presentOnly.Contains(name);

        public bool HasAssembly(string name) => _assemblies.Contains(name);

        public bool HasType(string name) => _members.ContainsKey(name) || _forwards.ContainsKey(name);

        public bool IsHidden(string name) => _hiddenTypes.Contains(name);

        public Access FindMember(string type, string key)
        {
            if (!_members.ContainsKey(type))
            {
                return Access.Public;
            }

            for (var current = type; current is not null && _members.TryGetValue(current, out var members); current = _bases[current])
            {
                if (members.TryGetValue(key, out var access))
                {
                    return access;
                }
            }

            return Access.Missing;
        }

        private Access? SecurityLevel(MetadataReader reader, CustomAttributeHandleCollection attributes, TypeDefinition? enclosing)
        {
            foreach (var handle in attributes)
            {
                var name = AttributeName(reader, reader.GetCustomAttribute(handle));

                if (name == "SecurityCriticalAttribute")
                {
                    return Access.Critical;
                }

                if (name == "SecuritySafeCriticalAttribute")
                {
                    return Access.Public;
                }
            }

            return enclosing is { } outer
                ? SecurityLevel(reader, outer.GetCustomAttributes(), outer.GetDeclaringType().IsNil ? null : reader.GetTypeDefinition(outer.GetDeclaringType()))
                : null;
        }

        private static string AttributeName(MetadataReader reader, CustomAttribute attribute) => attribute.Constructor.Kind switch
        {
            HandleKind.MemberReference when reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent.Kind == HandleKind.TypeReference =>
                reader.GetString(reader.GetTypeReference((TypeReferenceHandle)reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent).Name),
            HandleKind.MethodDefinition =>
                reader.GetString(reader.GetTypeDefinition(reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType()).Name),
            _ => "",
        };

        private static bool IsVisible(MetadataReader reader, TypeDefinition type)
        {
            var visibility = type.Attributes & TypeAttributes.VisibilityMask;

            if (visibility == TypeAttributes.Public)
            {
                return true;
            }

            return visibility is TypeAttributes.NestedPublic or TypeAttributes.NestedFamily or TypeAttributes.NestedFamORAssem
                && IsVisible(reader, reader.GetTypeDefinition(type.GetDeclaringType()));
        }

        private static string Join(string ns, string name) => ns.Length == 0 ? name : ns + "." + name;
    }

    private sealed class NameProvider(MetadataReader reader) : ISignatureTypeProvider<string, object?>
    {
        public string TypeDefinitionName(TypeDefinitionHandle handle)
        {
            var type = reader.GetTypeDefinition(handle);
            var name = reader.GetString(type.Name);
            var declaring = type.GetDeclaringType();

            if (!declaring.IsNil)
            {
                return TypeDefinitionName(declaring) + "+" + name;
            }

            var ns = reader.GetString(type.Namespace);
            return ns.Length == 0 ? name : ns + "." + name;
        }

        public string TypeReferenceName(TypeReferenceHandle handle)
        {
            var type = reader.GetTypeReference(handle);
            var name = reader.GetString(type.Name);

            if (type.ResolutionScope.Kind == HandleKind.TypeReference)
            {
                return TypeReferenceName((TypeReferenceHandle)type.ResolutionScope) + "+" + name;
            }

            var ns = reader.GetString(type.Namespace);
            return ns.Length == 0 ? name : ns + "." + name;
        }

        public string? OwnerName(EntityHandle handle) => handle.Kind switch
        {
            HandleKind.TypeReference => TypeReferenceName((TypeReferenceHandle)handle),
            HandleKind.TypeDefinition => TypeDefinitionName((TypeDefinitionHandle)handle),
            HandleKind.TypeSpecification => GenericOwner((TypeSpecificationHandle)handle),
            _ => null,
        };

        private string? GenericOwner(TypeSpecificationHandle handle)
        {
            var blob = reader.GetBlobReader(reader.GetTypeSpecification(handle).Signature);

            if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance)
            {
                return null;
            }

            blob.ReadCompressedInteger();
            return OwnerName(blob.ReadTypeHandle());
        }

        public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";

        public string GetByReferenceType(string elementType) => elementType + "&";

        public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
            genericType + "<" + string.Join(",", typeArguments) + ">";

        public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

        public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

        public string GetPinnedType(string elementType) => elementType;

        public string GetPointerType(string elementType) => elementType + "*";

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => "System." + typeCode switch
        {
            PrimitiveTypeCode.Void => "Void",
            PrimitiveTypeCode.Boolean => "Boolean",
            PrimitiveTypeCode.Char => "Char",
            PrimitiveTypeCode.SByte => "SByte",
            PrimitiveTypeCode.Byte => "Byte",
            PrimitiveTypeCode.Int16 => "Int16",
            PrimitiveTypeCode.UInt16 => "UInt16",
            PrimitiveTypeCode.Int32 => "Int32",
            PrimitiveTypeCode.UInt32 => "UInt32",
            PrimitiveTypeCode.Int64 => "Int64",
            PrimitiveTypeCode.UInt64 => "UInt64",
            PrimitiveTypeCode.Single => "Single",
            PrimitiveTypeCode.Double => "Double",
            PrimitiveTypeCode.String => "String",
            PrimitiveTypeCode.IntPtr => "IntPtr",
            PrimitiveTypeCode.UIntPtr => "UIntPtr",
            PrimitiveTypeCode.Object => "Object",
            PrimitiveTypeCode.TypedReference => "TypedReference",
            _ => typeCode.ToString(),
        };

        public string GetSZArrayType(string elementType) => elementType + "[]";

        public string GetTypeFromDefinition(MetadataReader _, TypeDefinitionHandle handle, byte rawTypeKind) => TypeDefinitionName(handle);

        public string GetTypeFromReference(MetadataReader _, TypeReferenceHandle handle, byte rawTypeKind) => TypeReferenceName(handle);

        public string GetTypeFromSpecification(MetadataReader _, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }
}
