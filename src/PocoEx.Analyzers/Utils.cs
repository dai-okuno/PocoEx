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
using System.Threading;
using System.Threading.Tasks;

namespace PocoEx
{
    internal static partial class Utils
    {
        public static readonly Task CompletedTask = Task.Run(() => { });

        public static Task<IEnumerable<ClassDeclarationSyntax>> GetClassDeclarationSyntaxAsync(this INamedTypeSymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<ClassDeclarationSyntax>(symbol, cancellationToken);

        private static Task<IEnumerable<TSyntax>> GetDeclarationSyntaxAsync<TSyntax>(this ISymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            where TSyntax : SyntaxNode
        {
            var declarations = symbol.DeclaringSyntaxReferences;
            var syntax = new Task<TSyntax>[declarations.Length];
            for (int i = 0; i < declarations.Length; i++)
            {
                syntax[i] = declarations[i].GetSyntaxAsync(cancellationToken).ContinueWith(t => t.IsCompleted ? (TSyntax)t.Result : default(TSyntax));
            }
            return Task.WhenAll(syntax).ContinueWith(t => t.Result.Where(node => node != default(TSyntax)));
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

        public static bool IsAncestorOf(this ITypeSymbol type, ITypeSymbol other)
        {
            while ((type = type.BaseType) != null)
            {
                if (type.Equals(other)) return true;
            }
            return false;
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

        public static TResult[] Convert<TSource, TResult>(this TSource[] source, Func<TSource, TResult> convert)
        {
            var result = new TResult[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = convert(source[i]);
            }
            return result;
        }
        public static string Signature(this IMethodSymbol method)
        {
            return new StringBuilder().AppendSignature(method).ToString();
        }

        public static StringBuilder AppendSignature(this StringBuilder builder, IMethodSymbol method)
        {
            builder.Append(method.Name).Append('(');
            var p = method.Parameters.GetEnumerator();
            if (p.MoveNext())
            {
                builder.AppendSignature(p.Current);
                while (p.MoveNext())
                {
                    builder.Append(", ").AppendSignature(p.Current);
                }
            }
            builder.Append(')');
            return builder;
        }

        private static StringBuilder AppendSignature(this StringBuilder builder, IParameterSymbol parameter)
        {
            switch (parameter.RefKind)
            {
                case RefKind.None:
                    break;
                case RefKind.Ref:
                    builder.Append("ref ");
                    break;
                case RefKind.Out:
                    builder.Append("out ");
                    break;
                default:
                    break;
            }
            builder.Append(parameter.Type);
            return builder;
        }

    }

}
