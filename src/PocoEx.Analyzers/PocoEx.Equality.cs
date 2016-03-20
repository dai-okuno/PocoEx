using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PocoEx.Linq;
using System.Threading.Tasks;
using System.Text;

namespace PocoEx
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class EqualityAnalyzer
        : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PocoEx.Equality";

        private static readonly ImmutableArray<SymbolKind> NamedTypeTargets
            = ImmutableArray.Create(SymbolKind.NamedType);

        private static readonly Action<CodeBlockAnalysisContext> _AnalyzeCodeBlock = AnalyzeCodeBlock;

        private static readonly Action<SymbolAnalysisContext> _AnalyzeNamedType = AnalyzeNamedType;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                Rules.PocoEx00101,
                Rules.PocoEx00102,
                Rules.PocoEx00103,
                Rules.PocoEx00104,
                Rules.PocoEx00105);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(_AnalyzeNamedType, NamedTypeTargets);
            context.RegisterCodeBlockAction(AnalyzeCodeBlock);
        }

        private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
        {
            var method = context.OwningSymbol as IMethodSymbol;
            if (method == null
                || method.DeclaredAccessibility != Accessibility.Public
                || method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 1
                || method.Name != "Equals") return;

            if (method.Parameters[0].Type.SpecialType == SpecialType.System_Object && method.IsOverride)
            {   // public override bool Equals(object obj)
                var analyzer = new EqualsObjectAnalyzer();
                analyzer.Analyze(context);
            }
            else
            {   // public bool Equals(TParameter parameter)

            }
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            var declarations = namedType.GetDeclarationSyntaxAsync();
            var iEquatable = context.Compilation.GetTypeByMetadataName(typeof(IEquatable<>).FullName);
            AnalyzePocoEx00101(context, namedType, declarations, iEquatable);
            if (namedType.IsValueType)
            {
                var iEquatableSelf = iEquatable.Construct(namedType);
                AnalyzePocoEx00102(context, namedType, declarations, iEquatableSelf);
            }
        }

        private static void AnalyzePocoEx00102(SymbolAnalysisContext context, INamedTypeSymbol namedType, Task<TypeDeclarationSyntax[]> declarations, INamedTypeSymbol iEquatableSelf)
        {
            foreach (var @interface in namedType.Interfaces)
            {
                if (iEquatableSelf.Equals(@interface))
                {   // implements IEquatable<namedType>
                    return;
                }
            }
            // IEquatable<namedType> is not implemented.
            declarations.Wait();
            context.ReportDiagnostic(Rules.PocoEx00102, declarations.Result.GetIdentifierLocations(), namedType.ToString());
        }

        private static void AnalyzePocoEx00101(SymbolAnalysisContext context, INamedTypeSymbol namedType, Task<TypeDeclarationSyntax[]> declarations, INamedTypeSymbol iEquatable)
        {
            foreach (var @interface in namedType.AllInterfaces)
            {
                if (!iEquatable.Equals(@interface.ConstructedFrom)) continue;
                // implements IEquatable<T>
                var methods = namedType.GetMembers(nameof(Equals));
                foreach (IMethodSymbol method in methods)
                {
                    if (method.DeclaredAccessibility == Accessibility.Public
                        && method.IsOverride
                        && method.Parameters.Length == 1
                        && method.Parameters[0].Type.SpecialType == SpecialType.System_Object)
                    {   // public override bool Equals(object)
                        return;
                    }
                }
                declarations.Wait();
                context.ReportDiagnostic(Rules.PocoEx00101, declarations.Result.GetIdentifierLocations());
                return;
            }
        }

        class EqualsObjectAnalyzer
            : CSharpSyntaxWalker
        {
            IEnumerable<IMethodSymbol> Uninvoked;

            SemanticModel SemanticModel;

            public void Analyze(CodeBlockAnalysisContext context)
            {
                var declarations = ((IMethodSymbol)context.OwningSymbol).GetDeclarationSyntaxAsync();

                SemanticModel = context.SemanticModel;
                var namedType = context.OwningSymbol.ContainingType;
                Uninvoked =
                    namedType.GetMembers(nameof(Equals))
                    .OfType<IMethodSymbol>()
                    .Where(m => m.DeclaredAccessibility == Accessibility.Public
                        && !m.IsStatic
                        && m.Parameters.Length == 1
                        && m.Parameters[0].Type.SpecialType != SpecialType.System_Object)
                    ;

                Visit(context.CodeBlock);
                declarations.Wait();
                var uninvoked = Format(Uninvoked);
                if (uninvoked != string.Empty)
                {
                    context.ReportDiagnostic(
                        Rules.PocoEx00103, declarations.Result, uninvoked);
                }
            }

            private string Format(IEnumerable<IMethodSymbol> methods)
            {
                var builder = new StringBuilder();
                using (var m = methods.GetEnumerator())
                {
                    if (!m.MoveNext()) return string.Empty;
                    builder.Append('\'').AppendSignature(m.Current).Append('\'');
                    while (m.MoveNext())
                    {
                        builder.Append(", ").Append('\'').AppendSignature(m.Current).Append('\'');
                    }
                }
                return builder.ToString();
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression.IsKind(SyntaxKind.IdentifierName)
                    && node.ArgumentList != null
                    && node.ArgumentList.Arguments.Count == 1
                    && ((IdentifierNameSyntax)node.Expression).Identifier.Text == nameof(Equals))
                {   // Equals(argument)
                    var method = SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                    if (method != null)
                    {
                        Uninvoked = Uninvoked.Remove(method);
                    }
                }
                base.VisitInvocationExpression(node);
            }
        }

        class EqualsReferenceTypeAnalyzer
            : CSharpSyntaxWalker
        {

        }
        class EqualsValueTypeAnalyzer
            : CSharpSyntaxWalker
        {

        }
    }
}