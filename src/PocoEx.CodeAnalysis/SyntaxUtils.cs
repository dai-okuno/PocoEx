using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.CodeAnalysis
{
    internal static class SyntaxUtils
    {

        public static SyntaxTrivia NewLine
            => SyntaxFactory.EndOfLine(FormattingOptions.NewLine.DefaultValue);
        
        public static SyntaxTriviaList Indent(this SyntaxTriviaList trivia, int level)
            => trivia.Add(SyntaxFactory.Whitespace(Utils.GetIndent(level)));
    }
}
