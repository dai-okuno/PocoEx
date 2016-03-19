using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;

namespace PocoEx.Analyzers.Test
{
    [TestClass]
    public class PocoEx00101Test : CodeFixVerifier
    {

        [TestMethod]
        public void Fix_PocoEx00101()
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
            return !ReferenceEquals(other, null);
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

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PocoExCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PocoExAnalyzer();
        }
    }
}
