using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
{
    [TestClass]
    public class PocoEx00102Test : DiagnosticVerifier
    {

        [TestMethod]
        public void Fix_non_generic()
        {
            int line = 5;
            int column = 12;
            var test = @"
using System;
namespace PocoEx
{
    struct Struct1
    {
        public int Value { get; private set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00102.Id,
                Message = string.Format(Resources.PocoEx00102MessageFormat, "PocoEx.Struct1"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Fix_generic()
        {
            int line = 5;
            int column = 12;
            var test = @"
using System;
namespace PocoEx
{
    struct Struct1<T>
        where T: struct
    {
        public T Value { get; private set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00102.Id,
                Message = string.Format(Resources.PocoEx00102MessageFormat, "PocoEx.Struct1<T>"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Analyze_implements_IEquatableSelf()
        {
            var test = @"
using System;
namespace PocoEx
{
    struct Struct1: IEquatable<Struct1>
    {
        public int Value { get; private set; }
        public bool Equals(Struct1 other)
            => Value == other.Value;
        public override bool Equals(object obj)
            => obj is Struct1 && Equals((Struct1)obj);
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

    }
}
