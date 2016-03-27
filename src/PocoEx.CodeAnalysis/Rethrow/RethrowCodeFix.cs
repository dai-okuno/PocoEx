using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.CodeAnalysis.Rethrow
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RethrowCodeFix)), Shared]
    public class RethrowCodeFix
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
            return Utils.Completed;
        }
    }
}
