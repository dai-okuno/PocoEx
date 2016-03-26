using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocoEx.CodeAnalysis.Equality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace PocoEx.CodeAnalysis.Test
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
        public void PocoEx00102_Fix_non_generic()
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
        public void PocoEx00102_Fix_generic()
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
        public void PocoEx00102_Analyze_implements_IEquatableSelf()
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
        public void PocoEx00103_Analyze_no_Equals()
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
        public void PocoEx00103_Analyze_NoDiagnostic_Class()
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
        public void PocoEx00103_Fix_reference_type()
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
        }

        [TestMethod]
        public void PocoEx00103_Fix_value_type()
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
        public void PocoEx00103_Fix_2_Equals()
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

        #endregion

        #region PocoEx00104

        [TestMethod]
        public void PocoEx00104_Analyze()
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

        #endregion

        #region PocoEx00105

        [TestMethod]
        public void PocoEx00105_Analyze()
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
            => !ReferenceEquals(other, null)
                && (Value == other.Value);

        public override bool Equals(object obj)
            => Equals(obj as Class1);

        public override int GetHashCode()
            => Value;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00105.Id,
                Message = string.Format(Resources.PocoEx00105MessageFormat, "other"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        #endregion

        #region PocoEx00106

        [TestMethod]
        public void PocoEx00106_Fix_self_1_public_property()
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
        public void PocoEx00106_Fix_self_2_public_properties()
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
        public void PocoEx00106_Fix_self_1_of_2_properties()
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
        public void PocoEx00106_Analyze_self_suppress_1_of_2_properties()
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

        [PocoEx.CodeAnalysis.EqualityIgnore(nameof(Value))]
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
        public void PocoEx00106_Fix_backing_field()
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
        #endregion

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EqualityAnalyzer();
        }

    }
}
