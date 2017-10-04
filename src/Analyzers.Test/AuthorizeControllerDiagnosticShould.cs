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
        public void RunFineOnNonController()
        {
            var source = @"public class DbContextProvider
        {
            private SqlConnection _connection;

            public string ScopeName { get; set; }
            public UserFilterContext UserFilterContext { get; set; }
            public readonly int LenderId = 100;

            public DbContextProvider(string scopeName = null, SqlConnection connection = null, UserFilterContext userFilterContext = null)
            {
                UserFilterContext = userFilterContext ?? new UserFilterContext { LenderId = LenderId };
                ScopeName = scopeName ?? Guid.NewGuid().ToString();
                _connection = connection;
            }
}
";
            VerifyCSharpDiagnostic(source);
        }
        

            [Test]
        public void WarnIfNoAttributeIsFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
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
                        new DiagnosticResultLocation("Test0.cs", 5, 18)
                    }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        [Test]
        public void WarnIfClassHasAttributesButNoAuthAttributeIsFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    [Route(""/home"")]
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
                        new DiagnosticResultLocation("Test0.cs", 6, 18)
                    }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        [Test]
        public void NotWarnIfAuthorizeAttributeIsOnlyOneFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
    }
}
";
            VerifyCSharpDiagnostic(source);
        }

        [Test]
        public void NotWarnIfAllowAnonymousAttributeIsOnlyOneFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
    }
}
";
            VerifyCSharpDiagnostic(source);
        }

        [Test]
        public void NotWarnIfAllowAnonymouseAttributeIsFoundAmongOthers()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    [AllowAnonymous]
    [Route(""/home"")]
    public class HomeController : Controller
    {
    }
}
";
            VerifyCSharpDiagnostic(source);
        }

        [Test]
        public void NotWarnIfAuthorizeAttributeIsFoundAmongOthers()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    [Authorize]
    [Route(""/home"")]
    public class HomeController : Controller
    {
    }
}
";
            VerifyCSharpDiagnostic(source);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AuthorizeControllerDiagnostic();
        }
    }
}