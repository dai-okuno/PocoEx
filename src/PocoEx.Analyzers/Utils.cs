using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx
{
    internal static class Utils
    {
        public static readonly Task CompletedTask = Task.Run(() => { });

        public static bool IsSameIdentifier(this SyntaxToken identifier, ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.IdentifierName)
                && identifier.Text == ((IdentifierNameSyntax)expression).Identifier.Text;
        }
    }
}
