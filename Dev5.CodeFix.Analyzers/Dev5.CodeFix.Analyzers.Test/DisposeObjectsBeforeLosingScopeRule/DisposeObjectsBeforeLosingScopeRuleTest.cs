using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Dev5.CodeFix.Analyzers.Test
{
    [TestClass]
    public class DisposeObjectsBeforeLosingScopeRuleTest : CodeFixVerifier
    {

        [TestMethod]
        public void AnalyzeSampleCode()
        {
            var test = System.IO.File.ReadAllText(Environment.CurrentDirectory + @"..\..\..\DisposeObjectsBeforeLosingScopeRule\SampleCode.cs");
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DisposeObjectsBeforeLosingScopeRule();
        }
    }
}