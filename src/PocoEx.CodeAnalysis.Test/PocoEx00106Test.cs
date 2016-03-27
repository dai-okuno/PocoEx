using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
{
    [TestClass]
    public class PocoEx00106Test : CodeFixVerifier
    {

        [TestMethod]
        public void Fix_self_1_public_property()
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
            => ReferenceEquals(other, this)
                || (!ReferenceEquals(other, null)
                );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00106.Id,
                Message = string.Format(Resources.PocoEx00106MessageFormat, "PocoEx.Class1", "Value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column),
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Fix_self_2_public_properties()
        {
            int line = 11;
            int column = 21;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Id { get; private set; }

        public int Value { get; private set; }

        public bool Equals(Class1 other)
            => ReferenceEquals(other, this)
                || (!ReferenceEquals(other, null)
                );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
";
            var expected1 = new DiagnosticResult
            {
                Id = Rules.PocoEx00106.Id,
                Message = string.Format(Resources.PocoEx00106MessageFormat, "PocoEx.Class1", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column),
                }
            };
            var expected2 = new DiagnosticResult
            {
                Id = Rules.PocoEx00106.Id,
                Message = string.Format(Resources.PocoEx00106MessageFormat, "PocoEx.Class1", "Value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column),
                }
            }; VerifyCSharpDiagnostic(test, expected1, expected2);
        }

        [TestMethod]
        public void Fix_self_1_of_2_properties()
        {
            int line = 11;
            int column = 21;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Id { get; private set; }

        public int Value { get; private set; }

        public bool Equals(Class1 other)
            => ReferenceEquals(other, this)
                || (!ReferenceEquals(other, null)
                    && Value == other.Value
                );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00106.Id,
                Message = string.Format(Resources.PocoEx00106MessageFormat, "PocoEx.Class1", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column),
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Analyze_self_suppress_1_of_2_properties()
        {
            var test = @"
using System;
using System.Diagnostics.CodeAnalysis;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        public int Id { get; private set; }

        public int Value { get; private set; }

        [PocoEx.CodeAnalysis.Equality.EqualityIgnore(nameof(Value))]
        public bool Equals(Class1 other)
            => ReferenceEquals(other, this)
                || (!ReferenceEquals(other, null)
                    && Id == other.Id
                );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Fix_backing_field()
        {
            int line = 12;
            int column = 21;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>
    {
        private int _Value;

        public int Value
            => _Value;

        public bool Equals(Class1 other)
            => ReferenceEquals(other, this)
                || (!ReferenceEquals(other, null)
                );

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00106.Id,
                Message = string.Format(Resources.PocoEx00106MessageFormat, "PocoEx.Class1", "_Value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column),
                }
            };
            VerifyCSharpDiagnostic(test, expected);
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
