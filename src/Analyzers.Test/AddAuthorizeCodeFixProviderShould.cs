using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using TestHelper;

namespace Analyzers.Test
{
    [TestFixture]
    public class AddAuthorizeCodeFixProviderShould : CodeFixVerifier
    {
        [Test]
        public void AddAuthorizeAttributeWhenNoOtherAttributesExist()
        {
            var source = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        public string Blah()
        {

        }
    }
}";
            var result = @"using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        [AuthorizeRight]
        public string Blah()
        {

        }
    }
}";
            VerifyCSharpFix(source, result, null, true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AddAuthorizeCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AuthorizeControllerDiagnostic();
        }
    }
}