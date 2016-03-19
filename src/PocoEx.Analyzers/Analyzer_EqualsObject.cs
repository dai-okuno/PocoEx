using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Immutable;
using PocoEx.Linq;

namespace PocoEx
{
    partial class PocoExAnalyzer
    {

        class EqualsObjectAnalyzer
            : CSharpSyntaxWalker
        {
            private static readonly Action<SymbolAnalysisContext> Analyze = (context) =>
            {
                if (context.Symbol.Kind != SymbolKind.NamedType) return;
                var analyzer = new EqualsObjectAnalyzer()
                {
                    NamedType = (INamedTypeSymbol)context.Symbol,
                    SemanticModel = context.Compilation.GetSemanticModel(context.Symbol.Locations[0].SourceTree),
                };
                analyzer.CollectEqualsMethods((INamedTypeSymbol)context.Symbol);
                analyzer.ReportDiagnostic(context);

            };

            private static readonly ImmutableArray<SymbolKind> TargetKinds = ImmutableArray.Create(SymbolKind.NamedType);

            private IMethodSymbol EqualsObject { get; set; }

            private Task<MethodDeclarationSyntax[]> EqualsObjectDeclarations { get; set; }

            private ArraySegment<IMethodSymbol> EqualsOthers { get; set; }

            private IMethodSymbol EqualsSelf { get; set; }

            private Task<Location[]> EqualsSelfLocations { get; set; }

            private INamedTypeSymbol NamedType { get; set; }

            private Task<TypeDeclarationSyntax[]> NamedTypeDeclarations { get; set; }

            private SemanticModel SemanticModel { get; set; }

            private IEnumerable<IMethodSymbol> Uninvoked { get; set; }

            public static void RegisterTo(AnalysisContext context)
            {
                context.RegisterSymbolAction(Analyze, TargetKinds);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression.IsKind(SyntaxKind.IdentifierName)
                    && node.ArgumentList != null && node.ArgumentList.Arguments.Count == 1
                    && ((IdentifierNameSyntax)node.Expression).Identifier.Text == nameof(Equals))
                {
                    var method = SemanticModel.GetSymbolInfo(node);
                    Uninvoked = Uninvoked.Remove((IMethodSymbol)method.Symbol);
                }
                else
                {
                    base.VisitInvocationExpression(node);
                }
            }

            private void ReportDiagnostic(SymbolAnalysisContext context)
            {
                if (NamedType.IsValueType && !IsEquatableImplemented(context.Compilation))
                {
                    NamedTypeDeclarations.Wait();
                    context.ReportDiagnostic(Rules.PocoEx00102, NamedTypeDeclarations.Result.GetIdentifierLocations(), NamedType);
                }
                if (0 < EqualsOthers.Count)
                {
                    if (EqualsObject == null)
                    {
                        NamedTypeDeclarations.Wait();
                        context.ReportDiagnostic(Rules.PocoEx00101, NamedTypeDeclarations.Result.GetIdentifierLocations());
                    }
                    else
                    {
                        Uninvoked = EqualsOthers;
                        EqualsObjectDeclarations.Wait();
                        var declarations = EqualsObjectDeclarations.Result;
                        for (int i = 0; i < declarations.Length; i++)
                        {
                            if (declarations[i].Body == null && declarations[i].ExpressionBody == null) continue;
                            VisitMethodDeclaration(declarations[i]);
                        }
                        if (Uninvoked.Any())
                        {
                            context.ReportDiagnostic(Rules.PocoEx00103, EqualsObjectDeclarations.Result);
                        }
                    }
                }
            }

            private void CollectEqualsMethods(INamedTypeSymbol namedType)
            {
                NamedType = namedType;
                NamedTypeDeclarations = NamedType.GetDeclarationSyntaxAsync();
                var members = NamedType.GetMembers(nameof(Equals));
                var equalsOthers = new IMethodSymbol[members.Length];
                int count = 0;
                foreach (var member in members)
                {
                    if (member.Kind != SymbolKind.Method) continue;
                    var method = (IMethodSymbol)member;
                    if (method.DeclaredAccessibility != Accessibility.Public) continue;
                    if (method.ReturnType.SpecialType != SpecialType.System_Boolean) continue;
                    if (method.Parameters.Length != 1) continue;
                    var parameter = method.Parameters[0];
                    if (parameter.Type.SpecialType == SpecialType.System_Object)
                    {
                        EqualsObject = method;
                        EqualsObjectDeclarations = method.GetDeclarationSyntaxAsync();
                    }
                    else
                    {
                        equalsOthers[count++] = method;
                        if (NamedType.Equals(parameter.Type))
                        {
                            EqualsSelf = method;
                        }
                    }
                }
                EqualsOthers = new ArraySegment<IMethodSymbol>(equalsOthers, 0, count);
            }
            private bool IsEquatableImplemented(Compilation compilation)
            {
                var equatableSelf = compilation.GetTypeByMetadataName(typeof(IEquatable<>).FullName).Construct(NamedType);
                foreach (var t in NamedType.Interfaces)
                {
                    if (equatableSelf.Equals(t)) return true;
                }
                return false;
            }
        }
    }
}
