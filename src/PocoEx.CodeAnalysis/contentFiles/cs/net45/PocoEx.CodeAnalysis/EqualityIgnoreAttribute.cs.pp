using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
namespace PocoEx.CodeAnalysis
{
    using System;
    [Conditional("CodeAnalysisOnly")]
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class EqualityIgnoreAttribute : System.Attribute
    {
        public EqualityIgnoreAttribute(params string[] memberNames)
        {
            MemberNames = memberName;
        }

        public global::System.Collections.Generic.IReadOnlyCollection<string> MemberNames { get; }

    }
}