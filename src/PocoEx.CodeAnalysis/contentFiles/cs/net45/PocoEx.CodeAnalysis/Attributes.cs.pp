using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
namespace PocoEx.CodeAnalysis.Equality
{
    using System;
    [Conditional("CodeAnalysisOnly")]
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class EqualityIgnoreAttribute : System.Attribute
    {
        public EqualityIgnoreAttribute(params string[] memberNames)
        {
            MemberNames = memberNames;
        }

        public global::System.Collections.Generic.IReadOnlyCollection<string> MemberNames { get; }

    }
}