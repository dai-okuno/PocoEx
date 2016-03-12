using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace PocoEx
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PocoExCodeFixProvider)), Shared]
    public partial class PocoExCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(nameof(Rules.PocoEx00001)); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == Rules.PocoEx00001.Id)
                {
                    PocoEx00001.Fix(context, diagnostic);
                }
            }
        }
    }
}