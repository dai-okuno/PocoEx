using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.CodeAnalysis
{
    internal static partial class Utils
    {
        public static readonly Task Completed = Task.FromResult(0);

        private static volatile string[] Indents = Enumerable.Range(0, 8)
            .Select(l => l == 0 ? string.Empty : CreateIndent(l))
            .ToArray();

        /// <summary>Append the signature of the specified method symbol.</summary>
        /// <param name="builder"><see cref="StringBuilder"/> to append.</param>
        /// <param name="method"><see cref="IMethodSymbol"/> to append.</param>
        /// <returns><see cref="StringBuilder"/>.</returns>
        public static StringBuilder AppendSignature(this StringBuilder builder, IMethodSymbol method)
        {
            builder.Append(method.Name);
            if (method.IsGenericMethod)
            {
                builder.Append('<');
                var a = method.TypeArguments.GetEnumerator();
                a.MoveNext();
                builder.Append(a.Current);
                while (a.MoveNext())
                {
                    builder.Append(", ").Append(a.Current);
                }
                builder.Append('>');
            }
            builder.Append('(');
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

        /// <summary>Gets indent string with specified level.</summary>
        /// <param name="level">The level of indent.</param>
        /// <returns></returns>
        public static string GetIndent(int level)
        {
            var indents = Indents;
            if (indents.Length <= level)
            {
                var newIndents = new string[level];
                indents.CopyTo(newIndents, 0);
                for (int l = indents.Length; l < newIndents.Length; l++)
                {
                    newIndents[l] = CreateIndent(l);
                }
                Indents = newIndents;
            }
            return indents[level];
        }

        public static bool IsImplements(this ITypeSymbol type, ITypeSymbol @interface)
        {
            if (@interface.TypeKind != TypeKind.Interface) return false;
            foreach (var t in type.AllInterfaces)
            {
                if (t.Equals(@interface)) return true;
            }
            return false;
        }

        public static bool IsSubTypeOf(this ITypeSymbol type, ITypeSymbol other)
        {
            var baseType = type;
            while ((baseType = baseType.BaseType) != null)
            {
                if (baseType.Equals(other)) return true;
            }
            return false;
        }

        public static Diagnostic ToDiagnostic(this DiagnosticDescriptor descriptor, Location location, params object[] messageArgs)
            => Diagnostic.Create(descriptor, location, messageArgs);

        public static Diagnostic ToDiagnostic(this DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object[] messageArgs)
        {
            var location = locations.FirstOrDefault();
            if (location == null)
            {
                throw new ArgumentException(string.Format("'{0}' is empty.", nameof(locations)), nameof(locations));
            }
            else
            {
                return Diagnostic.Create(descriptor, location, locations.Skip(1), messageArgs);
            }
        }


        public static Exception UnexpectedEnum<T>(T value)
        {
            return new Exception(string.Format("{0}.{1} is unexpected. ", typeof(T), value));
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

        private static string CreateIndent(int level)
           => new string(' ', Microsoft.CodeAnalysis.Formatting.FormattingOptions.IndentationSize.DefaultValue * level);

    }
}
