using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
{
    [TestClass]
    public class PocoEx00101Test : CodeFixVerifier
    {

        [TestMethod]
        public void Fix()
        {
            int line = 5;
            int column = 11;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Value { get; set; }

        public bool Equals(Class1 other)
        {
            return !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || Value == other.Value
                );
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00101.Id,
                Message = Resources.PocoEx00101MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);

            VerifyCSharpFix(test, @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Value { get; set; }

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public bool Equals(Class1 other)
        {
            return !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || Value == other.Value
                );
        }
    }
}", allowNewCompilerDiagnostics: true);
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
