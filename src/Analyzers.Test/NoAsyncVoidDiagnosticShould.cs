using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace Analyzers.Test
{
    [TestFixture]
    public class NoAsyncVoidDiagnosticShould : CodeFixVerifier
    {




        [Test]
        public void NotWarnOnAsyncTask()
        {
            var source = @"using System.Web.Mvc;
                               using System.Threading.Task;
namespace WebApplication5.Controllers
{
    public class HomeController
    {
        public async Task Blah(){

        }
    }
}
";
            VerifyCSharpDiagnostic(source);
        }


        [Test]
        public void WarnIfAsyncVoidFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController
    {
        public async void Blah(){

        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "Analyzers",
                Message = "HomeController.Blah has an async void signature.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 27)
                    }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NoAsyncVoidDiagnostic();
        }
    }
}

