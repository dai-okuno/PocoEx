using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace PocoEx.Analyzers.Test
{
    [TestClass]
    public class EqualityTest : CodeFixVerifier
    {
        [TestMethod]
        public void Empty()
        {
            var test = "";
            VerifyCSharpDiagnostic(test);
        }
        #region PocoEx00101

        [TestMethod]
        public void PocoEx00101_Fix()
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
        }

        #endregion

        #region PocoEx00102

        [TestMethod]
        public void PocoEx00102_Fix_NonGeneric()
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
        public void PocoEx00102_Fix_Generic()
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
        public void PocoEx00102_Analyze_Implements_IEquatableSelf()
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

        #endregion

        #region PocoEx00103

        [TestMethod]
        public void PocoEx00103_Analyze_NoEquals()
        {
            var test = @"
using System;
namespace PocoEx
{
    class Class1
    {
        public T Value { get; private set; }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void PocoEx00103_Analyze_NoDiagnostic_Class()
        {
            var test = @"
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
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void PocoEx00103_Fix_ReferenceType()
        {
            int line = 9;
            int column = 9;
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
        }

        [TestMethod]
        public void PocoEx00103_Fix_ValueType()
        {
            int line = 9;
            int column = 9;
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
        public void PocoEx00103_Fix_Multi()
        {
            int line = 9;
            int column = 9;
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

        #endregion

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EqualityAnalyzer();
        }
    }
}
