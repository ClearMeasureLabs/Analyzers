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
        public string Blah(){

        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "Analyzers",
                Message = "HomeController.Blah has no authorization attribute.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 23)
                    }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        [Test]
        public void NotWarnIfNoAttributeIsFoundAndMethodPrivate()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        private string Blah(){

        }
    }
}
";

            VerifyCSharpDiagnostic(source);
        }

        [Test]
        public void WarnIfMethodHasAttributesButNoAuthAttributeIsFound()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        [Route(""/home"")]
        public string Blah(){

        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "Analyzers",
                Message = "HomeController.Blah has no authorization attribute.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 23)
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
    public class HomeController : Controller
    {
        [AuthorizeRight]
        public string Blah(){

        }
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
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public string Blah(){

        }
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
    public class HomeController : Controller
    {
        [AllowAnonymous]
        [Route(""/home"")]
        public string Blah(){

        }
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
    public class HomeController : Controller
    {
        [Route(""/home"")]
        [AuthorizeRight]
        public string Blah(){

        }
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