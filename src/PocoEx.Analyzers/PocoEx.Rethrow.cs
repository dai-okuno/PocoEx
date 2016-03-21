using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace PocoEx
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RethrowAnalyzer
        : DiagnosticAnalyzer
    {

        public const string DiagnosticId = "PocoEx.Rethrow";

        private static readonly ImmutableArray<SyntaxKind> CatchClauseTargets
            = ImmutableArray.Create(SyntaxKind.CatchClause);

        private static readonly Action<SyntaxNodeAnalysisContext> _AnalyzeCatchClause = AnalyzeCatchClause;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Rules.PocoEx00001, Rules.PocoEx00002);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(_AnalyzeCatchClause, CatchClauseTargets);
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var @catch = (CatchClauseSyntax)context.Node;
            if (@catch.Declaration.Identifier == null) return;
            var analyzer = new CatchClauseAnalyzer()
            {
                Context = context,
                Identifier = @catch.Declaration.Identifier,
            };
            analyzer.VisitCatchClause(@catch);
        }

        class CatchClauseAnalyzer
            : CSharpSyntaxWalker
        {
            public SyntaxNodeAnalysisContext Context { get; set; }

            public SyntaxToken Identifier { get; set; }
            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                if (node.Expression == null)
                {
                    return;
                }
                else if (Identifier.IsSameIdentifier(node.Expression))
                {   // catch(Expression ex) { throw ex; }
                    Context.ReportDiagnostic(Rules.PocoEx00001, node, Identifier.Text);
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
                    Context.ReportDiagnostic(Rules.PocoEx00002, @new, @new.Type, Identifier.Text);
                }
            }

        }
    }
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PocoEx00001CodeFix)), Shared]
    public class PocoEx00001CodeFix
        : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(Rules.PocoEx00001.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == Rules.PocoEx00001.Id)
                {

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use 'throw;'",
                            async (cancellation) =>
                            {
                                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                                var node = root.FindNode(diagnostic.Location.SourceSpan);
                                return context.Document.WithSyntaxRoot(root.ReplaceNode(node, SyntaxFactory.ThrowStatement()));
                            },
                            equivalenceKey: nameof(Rules.PocoEx00001)),
                        diagnostic);
                }
            }
            return Utils.CompletedTask;
        }
    }
}