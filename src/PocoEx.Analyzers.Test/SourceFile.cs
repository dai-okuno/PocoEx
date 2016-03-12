using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PocoEx.Analyzers.Test
{
    class SourceFile
    {
        private static readonly char[] LineBreak = new[] { '\r', '\n' };

        private int LineOffset;
        private string CurrentIndent = "";
        private IDisposable EndBlock;
        private readonly Stack<string> Indents = new Stack<string>();
        private StringBuilder Source = new StringBuilder();
        /// <summary>Initialize new instance of <see cref="SourceFile"/> with the method 'static void Main(string[] args)'.</summary>
        /// <param name="body">The body of Main method.</param>
        /// <returns>An instance of <see cref="SourceFile"/> with the method 'static void Main(string[] args)'.</returns>
        /// <remarks>
        /// <code>
        /// namespace PocoEx
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             // body
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static SourceFile svm(string body)
        {
            var source = new SourceFile();
            using (source.Namespace("PocoEx"))
            {
                using (source.Class("Program"))
                {
                    using (source.Method("static void Main(string[] args)"))
                    {
                        source.WriteLines(body);
                    }
                }
            }
            return source;
        }

        /// <summary>Initialize new instance of <see cref="SourceFile"/> with the method 'static void Main(string[] args)'.</summary>
        /// <param name="body">The body of Main method.</param>
        /// <returns>An instance of <see cref="SourceFile"/> with the method 'static void Main(string[] args)'.</returns>
        /// <remarks>
        /// <code>
        /// namespace PocoEx
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             // body
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static SourceFile svm(string body, ref int line, ref int column)
        {
            var source = new SourceFile();
            using (source.Namespace("PocoEx"))
            {
                using (source.Class("Program"))
                {
                    using (source.Method("static void Main(string[] args)"))
                    {
                        source.WriteLines(body, ref line, ref column);
                    }
                }
            }
            return source;
        }
        public SourceFile()
        {
            EndBlock = new EndBlockImpl() { Source = this };
        }

        public void Import(string snippet)
        {
            WriteLine(CurrentIndent, "using ", snippet, ";");
        }

        public IDisposable Block()
        {
            return Block(null);
        }

        private IDisposable Block(params string[] snippets)
        {
            if (snippets != null && 0 < snippets.Length)
            {
                WriteLine(snippets);
            }
            WriteLine("{");
            Indent();
            return EndBlock;
        }
        public IDisposable Class(string name)
        {
            return Block("class ", name);
        }
        public IDisposable Method(string signature)
        {
            return Block(signature);
        }

        public IDisposable Namespace(string name)
        {
            return Block("namespace ", name);
        }
        public override string ToString()
        {
            return Source.ToString();
        }
        public void WriteLine()
        {
            Source.AppendLine();
            LineOffset++;
        }
        public void WriteLine(string snippet)
        {
            if (!string.IsNullOrEmpty(snippet))
            {
                Source.Append(CurrentIndent).Append(snippet);
            }
            WriteLine();
        }
        public void WriteLines(string snippets, ref int line, ref int column)
        {
            line += LineOffset;
            column += CurrentIndent.Length;
            WriteLines(snippets);
        }

        public void WriteLines(string snippets)
        {
            if (string.IsNullOrEmpty(snippets)) return;
            int offset = 0;
            int last = 0;
            while (0 <= (last = snippets.IndexOfAny(LineBreak, offset)))
            {   // 改行が見つかった場合
                int length = last + 1 - offset;
                if (1 < length)
                {   // 1文字以上ある場合
                    Source.Append(CurrentIndent);
                }
                if (snippets[last] == '\r' && last < snippets.Length && snippets[last + 1] == '\n')
                {   // \r\n である場合
                    length++;
                }
                Source.Append(snippets, offset, length);
                offset += length;
                LineOffset++;
            }
            if (offset < snippets.Length)
            {
                Source.Append(CurrentIndent).Append(snippets, offset, snippets.Length - offset);
            }
            WriteLine();
        }

        private void WriteLine(params string[] snippets)
        {
            if (snippets.Length < 1) return;
            Source.Append(CurrentIndent);
            for (int i = 0; i < snippets.Length; i++)
            {
                Source.Append(snippets[i]);
            }
            WriteLine();
        }
        private void Indent(string indent = "    ")
        {
            Indents.Push(indent);
            CurrentIndent += indent;
        }
        private void Unindent()
        {
            if (Indents.Count < 1) return;
            var unindent = Indents.Pop();
            CurrentIndent = CurrentIndent.Substring(0, CurrentIndent.Length - unindent.Length);
        }
        class EndBlockImpl : IDisposable
        {
            public SourceFile Source;
            public void Dispose()
            {
                Source.Unindent();
                Source.WriteLine("}");
            }
        }
    }
}
