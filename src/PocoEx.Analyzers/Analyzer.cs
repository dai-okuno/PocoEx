using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace PocoEx
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class PocoExAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PocoEx";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            typeof(Rules).GetRuntimeFields()
            .Where(f => f.IsStatic && f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => f.GetValue(null) as DiagnosticDescriptor)
            .ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            RethrowAnalyzer.RegisterTo(context);
            EqualsObjectAnalyzer.RegisterTo(context);
        }
    }
}
