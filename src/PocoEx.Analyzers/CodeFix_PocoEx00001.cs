using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PocoEx
{
    partial class PocoExCodeFixProvider
    {
        internal class PocoEx00001
        {
            public static void Fix(CodeFixContext context, Diagnostic diagnostic)
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
                        equivalenceKey: nameof(PocoEx00001)),
                    diagnostic);
            }
        }

    }
}
