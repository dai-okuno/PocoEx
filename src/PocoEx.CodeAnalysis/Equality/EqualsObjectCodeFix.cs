using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Microsoft.CodeAnalysis.Formatting;

namespace PocoEx.CodeAnalysis.Equality
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EqualsObjectCodeFix)), Shared]
    public class EqualsObjectCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(Rules.PocoEx00101.Id, Rules.PocoEx00103.Id);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                CodeAction action = Rewriter.CreateAction(context, diagnostic);
                context.RegisterCodeFix(action, diagnostic);

            }
            return Utils.Completed;
        }

        class Rewriter
        {
            CodeFixContext Context;
            Diagnostic Diagnostic;
            SemanticModel SemanticModel;

            public Rewriter()
            {
                RewriteAsync = _RewriteAsync;
            }

            public static CodeAction CreateAction(CodeFixContext context, Diagnostic diagnostic)
            {
                return CodeAction.Create(
                    diagnostic.Id == Rules.PocoEx00101.Id ? Resources.PocoEx00101CodeFixTitle : Resources.PocoEx00103CodeFixTitle,
                    new Rewriter()
                    {
                        Context = context,
                        Diagnostic = diagnostic,
                    }.RewriteAsync, diagnostic.Id);
            }

            private Func<CancellationToken, Task<Document>> RewriteAsync;
            private async Task<Document> _RewriteAsync(CancellationToken cancellation)
            {
                try
                {
                    var root = await Context.Document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
                    var node = root.FindNode(Diagnostic.Location.SourceSpan);
                    SemanticModel = await Context.Document.GetSemanticModelAsync(cancellation).ConfigureAwait(false);
                    if (Diagnostic.Id == Rules.PocoEx00101.Id)
                    {
                        var typeDeclaration = (TypeDeclarationSyntax)node;
                        root = root.InsertNodesBefore(
                            typeDeclaration.Members.First(n => n.IsKind(SyntaxKind.MethodDeclaration) && ((MethodDeclarationSyntax)n).Identifier.Text == nameof(Equals)),
                            new[]
                            {
                                CreateMethod(typeDeclaration)
                            });
                        return Context.Document.WithSyntaxRoot(root);
                    }
                    else if (Diagnostic.Id == Rules.PocoEx00103.Id)
                    {
                        var current = (MethodDeclarationSyntax)node;
                        root = root.ReplaceNode(current, CreateMethod((TypeDeclarationSyntax)current.Parent));
                        return Context.Document.WithSyntaxRoot(root);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("'{0}' is not supported.", Diagnostic.Id));
                    }
                }
                catch (TaskCanceledException)
                {
                    return Context.Document;
                }

            }

            private MethodDeclarationSyntax CreateMethod(TypeDeclarationSyntax typeDeclaration)
            {
                var obj = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), SyntaxFactory.Identifier("obj"), default(EqualsValueClauseSyntax));
                var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), nameof(Equals))
                    .WithModifiers(new SyntaxTokenList().AddRange(new[] {
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.OverrideKeyword), }))
                    .WithParameterList(SyntaxFactory.ParameterList(new SeparatedSyntaxList<ParameterSyntax>().Add(obj)))
                    .NormalizeWhitespace()
                    .WithLeadingTrivia(typeDeclaration.GetLeadingTrivia().Insert(0, SyntaxUtils.NewLine).Indent(1))
                    .WithExpressionBody(CreateMethodBody(typeDeclaration, obj))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    .WithTrailingTrivia(SyntaxUtils.NewLine)
                    ;

                return method.WithoutAnnotations(Formatter.Annotation);
            }

            private ArrowExpressionClauseSyntax CreateMethodBody(TypeDeclarationSyntax typeDeclaration, ParameterSyntax obj)
            {
                var indent = typeDeclaration.GetLeadingTrivia()
                            .Insert(0, SyntaxUtils.NewLine)
                            .Indent(2)
                            ;
                ExpressionSyntax body = null;
                var type = SemanticModel.GetDeclaredSymbol(typeDeclaration);
                foreach (var method in typeDeclaration.Members
                   .OfType<MethodDeclarationSyntax>()
                   .Select(method => new { Symbol = SemanticModel.GetDeclaredSymbol(method), Declaration = method })
                   .Where(method => EqualityAnalyzer.IsEqualsT(method.Symbol) && method.Symbol.Parameters[0].Type.SpecialType != SpecialType.System_Object))
                {
                    ExpressionSyntax expression;
                    if (method.Symbol.Parameters[0].Type.IsValueType)
                    {
                        expression = SyntaxFactory.ParenthesizedExpression(
                            SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression,
                                SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression,
                                    SyntaxFactory.IdentifierName(obj.Identifier),
                                    method.Declaration.ParameterList.Parameters[0].Type),
                                SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(nameof(Equals)))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(SyntaxFactory.CastExpression(
                                            method.Declaration.ParameterList.Parameters[0].Type,
                                            SyntaxFactory.IdentifierName(obj.Identifier)))))
                            .NormalizeWhitespace());

                    }
                    else
                    {
                        expression = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(nameof(Equals)))
                            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.BinaryExpression(SyntaxKind.AsExpression,
                            SyntaxFactory.IdentifierName(obj.Identifier),
                            method.Declaration.ParameterList.Parameters[0].Type)))
                            .NormalizeWhitespace();
                    }
                    if (body == null)
                    {
                        body = expression.WithLeadingTrivia(SyntaxFactory.Space);
                        indent = indent.Indent(1);
                    }
                    else
                    {
                        body = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression,
                            body,
                            SyntaxFactory.Token(SyntaxKind.BarBarToken).WithLeadingTrivia(indent).WithTrailingTrivia(SyntaxFactory.Space),
                            expression);
                    }
                }
                indent = indent.RemoveAt(indent.Count - 1);
                return SyntaxFactory.ArrowExpressionClause(body.WithLeadingTrivia(SyntaxFactory.Space))
                    .WithLeadingTrivia(indent);
            }
        }
    }
}