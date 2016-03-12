using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace PocoEx
{
    partial class PocoExAnalyzer
    {
        internal class RethrowAnalyzer
            : CSharpSyntaxWalker
        {
            private static readonly Action<SyntaxNodeAnalysisContext> Analyze = (context) =>
            {
                var @catch = (CatchClauseSyntax)context.Node;
                if (@catch.Declaration.Identifier == null) return;
                var analyzer = new RethrowAnalyzer()
                {
                    Context = context,
                    Identifier = @catch.Declaration.Identifier,
                };
                analyzer.VisitCatchClause(@catch);
            };

            private static readonly ImmutableArray<SyntaxKind> TargetKinds = ImmutableArray.Create(SyntaxKind.CatchClause);

            public SyntaxNodeAnalysisContext Context { get; set; }

            public SyntaxToken Identifier { get; set; }

            public static void RegisterTo(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(Analyze, TargetKinds);
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                if (node.Expression == null)
                {
                    return;
                }
                else if (Identifier.IsSameIdentifier(node.Expression))
                {   // catch(Expression ex) { throw ex; }
                    Context.ReportDiagnostic(Diagnostic.Create(Rules.PocoEx00001, node.GetLocation(), Identifier.Text));
                }
                else if (node.Expression.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var @new = (ObjectCreationExpressionSyntax)node.Expression;
                    foreach (var arg in @new.ArgumentList.Arguments)
                    {
                        if (Identifier.IsSameIdentifier(arg.Expression))
                        {   // throw new WrapperException(arguments, ex);
                            return;
                        }
                    }
                    Context.ReportDiagnostic(Diagnostic.Create(Rules.PocoEx00002, @new.GetLocation(), @new.Type, Identifier.Text));
                }
            }
        }
    }
}
