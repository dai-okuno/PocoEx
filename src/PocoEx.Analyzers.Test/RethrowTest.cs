using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace PocoEx.Analyzers.Test
{
    [TestClass]
    public class RethrowTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void Empty()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Fix_PocoEx00001()
        {
            int line = 5;
            int column = 5;
            var test = SourceFile.svm(@"
try { }
catch (Exception ex)
{
    throw ex;
}", ref line, ref column).ToString();
            var expected = new DiagnosticResult
            {
                Id = Rules.PocoEx00001.Id,
                Message = string.Format(Resources.PocoEx00001MessageFormat, "ex"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);

            var fixtest = SourceFile.svm(@"
try { }
catch (Exception ex)
{
    throw;
}");
            VerifyCSharpFix(test, fixtest.ToString(), allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PocoEx00001CodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RethrowAnalyzer();
        }
    }
}