using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoAsyncVoidDiagnostic : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Analyzers";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = "Async void methods are not allowed.";
        private static readonly LocalizableString MessageFormat = "{0} has an async void signature.";

        private static readonly LocalizableString Description =
            "Async void is an anti-pattern, please return a Task instead.";

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
                var returnType = methodDeclarationSyntax.ReturnType as PredefinedTypeSyntax;
                if (returnType.Keyword.Kind() == SyntaxKind.VoidKeyword && methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    var classDeclarationSyntax = methodDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().LastOrDefault();
                    if (classDeclarationSyntax == null ) return;

                    var className = classDeclarationSyntax.Identifier.Value.ToString();
                    var methodName = methodDeclarationSyntax.Identifier.Value.ToString();
                    var diagnosticLocation = methodDeclarationSyntax.Identifier.GetLocation();
                    var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, $"{className}.{methodName}");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}