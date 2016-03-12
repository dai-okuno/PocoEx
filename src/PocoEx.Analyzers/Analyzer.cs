using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PocoEx
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class PocoExAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PocoEx";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rules.PocoEx00001,
            Rules.PocoEx00002);

        public override void Initialize(AnalysisContext context)
        {
            RethrowAnalyzer.RegisterTo(context);
        }
    }
}
