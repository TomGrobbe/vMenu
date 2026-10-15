using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace vMenu.Enhanced.PluginAnalyzers;

/// <summary>Warns about the two ways a plugin can take the screen away from the player.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MenuPriorityAnalyzer : DiagnosticAnalyzer
{
    public const string HighPriorityId = "VMENU001";

    public const string DoNotCloseId = "VMENU002";

    private const string HelpLink = "https://docs.vespura.com/vmenu/enhanced/plugins/standalone-menus/";

    private const string PluginMenu = "vMenu.Enhanced.ClientAPI.PluginMenu";

    private const string RequestOpen = "RequestOpenAsync";

    private static readonly DiagnosticDescriptor HighPriority = new(
        HighPriorityId,
        "Menu opened at high priority",
        "Only use this in extreme situations, prevents vMenu or other plugins with a lower priority from opening any menus while yours is open. Use responsibly!",
        "vMenu",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);

    private static readonly DiagnosticDescriptor DoNotClose = new(
        DoNotCloseId,
        "Menu opened with doNotClose",
        "Only use this in extreme situations. It blocks the player from using anything else, so always close the menu as soon as possible.",
        "vMenu",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(HighPriority, DoNotClose);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (method.Name != RequestOpen || method.ContainingType?.ToDisplayString() != PluginMenu)
        {
            return;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { } parameter || argument.ArgumentKind == ArgumentKind.DefaultValue)
            {
                continue;
            }

            switch (parameter.Name)
            {
                case "priority" when MaybeHigh(argument.Value, parameter.Type):
                    context.ReportDiagnostic(Diagnostic.Create(HighPriority, argument.Syntax.GetLocation()));
                    break;

                case "doNotClose" when !IsConstant(argument.Value, false):
                    context.ReportDiagnostic(Diagnostic.Create(DoNotClose, argument.Syntax.GetLocation()));
                    break;
            }
        }
    }

    // A value only known at runtime could be high, and a warning is cheaper than a player stuck in a menu.
    private static bool MaybeHigh(IOperation value, ITypeSymbol priorityType)
    {
        if (!value.ConstantValue.HasValue)
        {
            return true;
        }

        foreach (var member in priorityType.GetMembers("High"))
        {
            if (member is IFieldSymbol { HasConstantValue: true } high)
            {
                return Equals(high.ConstantValue, value.ConstantValue.Value);
            }
        }

        return false;
    }

    private static bool IsConstant(IOperation value, bool expected) =>
        value.ConstantValue is { HasValue: true, Value: bool actual } && actual == expected;
}
