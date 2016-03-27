using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
{
    [TestClass]
    public class PocoEx00104Test : CodeFixVerifier
    {

        [TestMethod]
        public void Analyze_class()
        {
            int line = 9;
            int column = 21;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Value { get; private set; }

        public bool Equals(Class1 other)
            => (ReferenceEquals(other, this)
                || (Value == other.Value)
            );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00104.Id,
                Message = string.Format(Resources.PocoEx00104MessageFormat, "other"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void Analyze_struct()
        {
            var test = @"
using System;
namespace PocoEx
{
    struct Class1: IEquatable<Class1>
    {
        public int Value { get; private set; }

        public bool Equals(Class1 other)
            => (Value == other.Value);

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EqualityAnalyzer();
        }
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new EqualsObjectCodeFix();
        }
    }
}
