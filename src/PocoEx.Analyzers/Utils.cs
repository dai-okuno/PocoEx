using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PocoEx.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx
{
    internal static partial class Utils
    {
        public static readonly Task CompletedTask = Task.Run(() => { });
        public static Task<ClassDeclarationSyntax[]> GetClassDeclarationSyntaxAsync(this INamedTypeSymbol symbol)
            => GetDeclarationSyntaxAsync<ClassDeclarationSyntax>(symbol);

        private static Task<TSyntax[]> GetDeclarationSyntaxAsync<TSyntax>(this ISymbol symbol)
            where TSyntax : SyntaxNode
        {
            var declarations = symbol.DeclaringSyntaxReferences;
            var syntax = new Task<TSyntax>[declarations.Length];
            for (int i = 0; i < declarations.Length; i++)
            {
                syntax[i] = declarations[i].GetSyntaxAsync().ContinueWith(t => (TSyntax)t.Result);
            }
            return Task.WhenAll(syntax);
        }

        public static StringBuilder Join(this StringBuilder builder, string separator, IEnumerable<string> values)
        {
            using (var value = values.GetEnumerator())
            {
                if (!value.MoveNext()) return builder;
                builder.Append(value.Current);
                while (value.MoveNext())
                {
                    builder.Append(separator).Append(value.Current);
                }
            }
            return builder;
        }

        public static bool IsParameterAndNull(ParameterSyntax parameter, ExpressionSyntax x, ExpressionSyntax y)
        {
            return IsParameterAndNullCore(parameter, x, y)
                || IsParameterAndNullCore(parameter, y, x);

        }

        private static bool IsParameterAndNullCore(ParameterSyntax parameter, ExpressionSyntax x, ExpressionSyntax y)
        {
            return x.IsKind(SyntaxKind.NullLiteralExpression)
                && y.IsKind(SyntaxKind.IdentifierName)
                && ((IdentifierNameSyntax)y).Identifier.Text == parameter.Identifier.Text;
        }

        public static bool IsParameterAndThis(ParameterSyntax parameter, ExpressionSyntax x, ExpressionSyntax y)
        {
            return IsParameterAndThisCore(parameter, x, y)
                || IsParameterAndThisCore(parameter, y, x);
        }

        private static bool IsParameterAndThisCore(ParameterSyntax parameter, ExpressionSyntax x, ExpressionSyntax y)
        {
            return x.IsKind(SyntaxKind.ThisExpression)
                && y.IsKind(SyntaxKind.IdentifierName)
                && ((IdentifierNameSyntax)y).Identifier.Text == parameter.Identifier.Text;
        }

        public static bool IsReferenceEquals(InvocationExpressionSyntax invocation)
        {
            var identifierName = invocation.Expression as IdentifierNameSyntax;
            if (identifierName == null) return false;
            return identifierName.Identifier.Text == nameof(ReferenceEquals)
                && invocation.ArgumentList != null
                && invocation.ArgumentList.Arguments.Count == 2;
        }

        public static bool IsSameIdentifier(this SyntaxToken identifier, ExpressionSyntax expression)
            => expression.IsKind(SyntaxKind.IdentifierName)
            && identifier.Text == ((IdentifierNameSyntax)expression).Identifier.Text;

        public static void ReportDiagnostic<TSyntax>(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, TSyntax[] nodes, params object[] messageArgs)
            where TSyntax : SyntaxNode
        {
            if (nodes.Length == 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, nodes[0].GetLocation(), messageArgs));
            }
            else if (1 < nodes.Length)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, nodes[0].GetLocation(), nodes.Skip(1).Select(node => node.GetLocation()), messageArgs));
            }
        }
        public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location[] locations, params object[] messageArgs)
        {
            if (locations.Length == 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, locations[0], messageArgs));
            }
            else if (1 < locations.Length)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, locations[0], locations.Skip(1), messageArgs));
            }
        }

        public static TResult[] Convert<TSource, TResult>(this TSource[] source, Func<TSource, TResult> convert)
        {
            var result = new TResult[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = convert(source[i]);
            }
            return result;
        }

    }

}
