using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAuthorizeCodeFixProvider)), Shared]
    public class AddAuthorizeCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add Authorize Attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AuthorizeControllerDiagnostic.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var classDeclarationSyntax =
                root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => AddAuthorizeAttribute(context.Document, classDeclarationSyntax, root, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private Task<Solution> AddAuthorizeAttribute(Document document,
            ClassDeclarationSyntax classDeclarationSyntax, SyntaxNode root,
            CancellationToken cancellationToken)
        {
            var attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Authorize"));
            var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributeSyntax));
            var attributeLists = classDeclarationSyntax.AttributeLists.Add(attributeListSyntax.NormalizeWhitespace());

            var newRootWithAuthorizeAttribute = root.ReplaceNode(classDeclarationSyntax,
                classDeclarationSyntax.WithAttributeLists(attributeLists));

            var formattedRoot = Formatter.Format(newRootWithAuthorizeAttribute, new AdhocWorkspace());

            var newSolution = document.WithSyntaxRoot(formattedRoot).Project.Solution;

            return Task.FromResult(newSolution);
        }
    }
}