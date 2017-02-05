using System;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using TestHelper;

namespace Analyzers.Test
{
    [TestFixture]
    public class AuthorizeControllerDiagnosticShould : CodeFixVerifier
    {
        [Test]
        public void WarnIfNoAttributeIsFound()
        {
            var source = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers
{
    public class HomeController : Controller
    {
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "Analyzers",
                Message = "HomeController has no authorization attribute.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 5)
                    }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

      
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AuthorizeControllerDiagnostic();
        }
    }
}