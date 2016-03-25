using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PocoEx.CodeAnalysis.Rethrow
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RethrowAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PocoEx.Rethrow";

        private static readonly ImmutableArray<SyntaxKind> CatchClauseTargets
            = ImmutableArray.Create(SyntaxKind.CatchClause);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Rules.PocoEx00001, Rules.PocoEx00002);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, CatchClauseTargets);
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var @catch = (CatchClauseSyntax)context.Node;
            if (@catch.Declaration.Identifier == null) return;

            var cause = context.SemanticModel.GetDeclaredSymbol(@catch.Declaration);
            var analyzer = new CatchClauseAnalyzer()
            {
                Context = context,
                Cause = cause,
            };
            analyzer.VisitCatchClause(@catch);
        }

        class CatchClauseAnalyzer
            : CSharpSyntaxWalker
        {
            public SyntaxNodeAnalysisContext Context { get; set; }
            /// <summary><see cref="ILocalSymbol"/> of the catching exception.</summary>
            public ILocalSymbol Cause { get; set; }
            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                if (node.Expression == null)
                {
                    return;
                }
                else if (node.Expression.IsKind(SyntaxKind.IdentifierName))
                {
                    var symbol = Context.SemanticModel.GetSymbolInfo(node.Expression);
                    if (symbol.CandidateReason == CandidateReason.None && Cause.Equals(symbol.Symbol))
                    {
                        Context.ReportDiagnostic(Rules.PocoEx00001.ToDiagnostic(node.GetLocation(), Cause.Name));
                    }
                }
                else if (node.Expression.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var @new = (ObjectCreationExpressionSyntax)node.Expression;
                    var foundCause = false;
                    foreach (var arg in @new.ArgumentList?.Arguments ?? Enumerable.Empty<ArgumentSyntax>())
                    {
                        if (!arg.Expression.IsKind(SyntaxKind.IdentifierName)) continue;
                        var symbol = Context.SemanticModel.GetSymbolInfo(arg.Expression);
                        if (symbol.CandidateReason != CandidateReason.None) continue;
                        if (Cause.Equals(symbol.Symbol))
                        {
                            foundCause = true;
                            break;
                        }
                    }
                    if (!foundCause)
                    {
                        Context.ReportDiagnostic(Rules.PocoEx00002.ToDiagnostic(@new.GetLocation(), @new.Type, Cause.Name));
                    }
                }
            }

        }
    }
}