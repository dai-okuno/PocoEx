using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace PocoEx.Analyzers.Test
{
    [TestClass]
    public class PocoEx00001Test : CodeFixVerifier
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
}", ref line, ref column);
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
            var source = test.ToString();
            VerifyCSharpDiagnostic(source, expected);

            var fixtest = SourceFile.svm(@"
try { }
catch (Exception ex)
{
    throw;
}");
            VerifyCSharpFix(test.ToString(), fixtest.ToString(), allowNewCompilerDiagnostics: true);
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