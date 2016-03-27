using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
{
    [TestClass]
    public class PocoEx00103Test : CodeFixVerifier
    {

        [TestMethod]
        public void Analyze_no_Equals()
        {
            var test = @"
using System;
namespace PocoEx
{
    class Class1
    {
        public int Value { get; private set; }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_NoDiagnostic_Class()
        {
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>, IEquatable<int>
    {
        public int Value { get; private set; }

        public override bool Equals(object obj)
            => Equals(obj as Class1)
                || (obj is int && Equals((int)obj));

        public bool Equals(Class1 other)
            => !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || (Value == other.Value)
                );
        public bool Equals(int other)
            => Value == other;

        public override int GetHashCode()
            => Value;
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Fix_reference_type()
        {
            int line = 9;
            int column = 30;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>, IEquatable<int>
    {
        public T Value { get; private set; }

        public override bool Equals(object obj)
            => (obj is int && Equals((int)obj));

        public bool Equals(Class1 other)
            => !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || (Value == other.Value)
                );

        public bool Equals(int other)
            => Value == other;

        public override int GetHashCode()
            => Value;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00103.Id,
                Message = string.Format(Resources.PocoEx00103MessageFormat, "'Equals(PocoEx.Class1)'"),
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
    class Class1: IEquatable<Class1>, IEquatable<int>
    {
        public T Value { get; private set; }

        public override bool Equals(object obj)
            => Equals(obj as Class1)
                || (obj is int && Equals((int)obj));

        public bool Equals(Class1 other)
            => !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || (Value == other.Value)
                );

        public bool Equals(int other)
            => Value == other;

        public override int GetHashCode()
            => Value;
    }
}", allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void Fix_value_type()
        {
            int line = 9;
            int column = 30;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>, IEquatable<int>
    {
        public T Value { get; private set; }

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public bool Equals(Class1 other)
            => !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || (Value == other.Value)
                );
        public bool Equals(int other)
            => Value == other;

        public override int GetHashCode()
            => Value;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00103.Id,
                Message = string.Format(Resources.PocoEx00103MessageFormat, "'Equals(int)'"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Fix_2_Equals()
        {
            int line = 9;
            int column = 30;
            var test = @"
using System;
namespace PocoEx
{
    class Class1: IEquatable<Class1>, IEquatable<int>
    {
        public T Value { get; private set; }

        public override bool Equals(object obj)
            => base.Equals(obj);

        public bool Equals(Class1 other)
            => !ReferenceEquals(other, null)
                && (ReferenceEquals(other, this)
                    || (Value == other.Value)
                );

        public bool Equals(int other)
            => Value == other;

        public override int GetHashCode()
            => Value;

    }
}";
            var expected1 = new DiagnosticResult
            {
                Id = Rules.PocoEx00103.Id,
                Message = string.Format(Resources.PocoEx00103MessageFormat, "'Equals(PocoEx.Class1)', 'Equals(int)'"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected1);
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
