using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx
{
    internal class CatchClauseWalker : CSharpSyntaxWalker
    {
        private static readonly ImmutableArray<SyntaxKind> TargetKinds = ImmutableArray.Create(SyntaxKind.CatchClause);

        private static readonly Action<SyntaxNodeAnalysisContext> Analyze = (context) =>
        {
            var @catch = (CatchClauseSyntax)context.Node;
            if (@catch.Declaration.Identifier == null) return;
            var walker = new CatchClauseWalker()
            {
                Identifier = @catch.Declaration.Identifier,
                Context = context,
            };
        };
        public SyntaxToken Identifier { get; set; }
        public SyntaxNodeAnalysisContext Context { get; set; }


        public static void RegisterTo(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CatchClause);
        }
        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            if (node.Expression == null)
            {
                return;
            }
            else if (EqualsIdentifier(node.Expression))
            {
                Context.ReportDiagnostic(Diagnostic.Create(Rules.PocoEx00001, node.GetLocation(), Identifier.Text));
            }
            else if (node.Expression.IsKind(SyntaxKind.ObjectCreationExpression))
            {
                var @new = (ObjectCreationExpressionSyntax)node.Expression;
                foreach (var arg in @new.ArgumentList.Arguments)
                {
                    if (EqualsIdentifier(arg.Expression))
                    {
                        return;
                    }
                }
                Context.ReportDiagnostic(Diagnostic.Create(Rules.PocoEx00002, @new.GetLocation(), @new.Type, Identifier.Text));
            }
        }
        private bool EqualsIdentifier(ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.IdentifierName)
                && Identifier.Text == ((IdentifierNameSyntax)expression).Identifier.Text;
        }

    }
}
