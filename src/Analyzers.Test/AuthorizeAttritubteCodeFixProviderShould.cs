using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using TestHelper;

namespace Analyzers.Test
{
    [TestFixture]
    public class AuthorizeAttritubteCodeFixProviderShould : CodeFixVerifier
    {
        [Test]
        public void AddAuthorizeAttributeWhenNoOtherAttributesExist()
        {
            var source = @"using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers
{
    public class HomeController : Controller
    {
    }
}";
            var result = @"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
    }
}";
            VerifyCSharpFix(source, result, null, true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AuthorizeControllerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AuthorizeControllerDiagnostic();
        }
    }
}