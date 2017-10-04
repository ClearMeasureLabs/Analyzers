using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AuthorizeControllerDiagnostic : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Analyzers";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = "Controller found with no explicit authorization attribute.";
        private static readonly LocalizableString MessageFormat = "{0} has no authorization attribute.";

        private static readonly LocalizableString Description =
            "All controllers should have either AllowAnonymous or AuthorizeRight attributes.";

        private const string Category = "Controllers";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationSyntax = context.Node as MethodDeclarationSyntax;

            if (methodDeclarationSyntax != null )
            {
                var classDeclarationSyntax = methodDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();
                var hasControllerAsBaseType = classDeclarationSyntax.BaseList.Types.Any(bst =>
                {
                    var simpleBaseTypeSyntax = bst as SimpleBaseTypeSyntax;

                    var identifierNameSyntax = simpleBaseTypeSyntax?.Type as IdentifierNameSyntax;
                    if (identifierNameSyntax == null) return false;

                    return identifierNameSyntax.Identifier.Value.ToString() == "Controller";
                });

                if (!hasControllerAsBaseType) return;

                //only test public methods
                if (!methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword)) return;

                var hasAuthorizedOrAllowAnonymousAttribute = methodDeclarationSyntax.AttributeLists.Any(als =>
                {
                    return als.Attributes.Any(attributeSyntax =>
                    {
                        var attributeName = attributeSyntax.Name.ToString();
                        return attributeName == "AllowAnonymous" || attributeName == "AuthorizeRight";
                    });
                });

                if (hasAuthorizedOrAllowAnonymousAttribute) return;

                var className = classDeclarationSyntax.Identifier.Value.ToString();
                var methodName = methodDeclarationSyntax.Identifier.Value.ToString();
                var diagnosticLocation = methodDeclarationSyntax.Identifier.GetLocation();
                var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, $"{className}.{methodName}");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}