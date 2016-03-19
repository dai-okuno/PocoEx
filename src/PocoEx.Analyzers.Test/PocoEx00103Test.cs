using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace PocoEx.Analyzers.Test
{
    [TestClass]
    public class PocoEx00103Test : CodeFixVerifier
    {

        [TestMethod]
        public void No_Equals()
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
        public void MyTestMethod()
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
