using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SealAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypeSealedCodeFixProvider)), Shared]
public class MakeTypeSealedCodeFixProvider : CodeFixProvider
{
    public override sealed ImmutableArray<string> FixableDiagnosticIds => ["SEAL001"];

    public override sealed FixAllProvider GetFixAllProvider() => 
        WellKnownFixAllProviders.BatchFixer;

    public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null) return;
            
        var diagnostic = context.Diagnostics.First();
        var span = diagnostic.Location.SourceSpan;

        // Get the syntax node (works for both ClassDeclarationSyntax and RecordDeclarationSyntax)
        var typeDeclaration = root.FindToken(span.Start).Parent?.AncestorsAndSelf()
            .OfType<TypeDeclarationSyntax>().First();
            
        if (typeDeclaration == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Seal type",
                createChangedDocument: c => SealTypeAsync(context.Document, typeDeclaration, c),
                equivalenceKey: nameof(MakeTypeSealedCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> SealTypeAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
    {
        // Define tokens
        var sealedToken = SyntaxFactory.Token(SyntaxKind.SealedKeyword).WithTrailingTrivia(SyntaxFactory.Space);

        // Calculate insertion index
        // We want to insert after: public, private, protected, internal, static
        // We want to insert before: partial, unsafe, new
        // Standard C# order: [Access] [static/sealed/abstract] [partial] class
        
        var modifiers = typeDecl.Modifiers;
        var insertIndex = 0;

        for (var i = 0; i < modifiers.Count; i++)
        {
            var kind = modifiers[i].Kind();
            if (kind is SyntaxKind.PublicKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.InternalKeyword)
            {
                // Insert after the last access modifier
                insertIndex = i + 1;
            }
        }

        // Insert 'sealed' at the calculated position
        var newModifiers = modifiers.Insert(insertIndex, sealedToken);
        var newTypeDecl = typeDecl.WithModifiers(newModifiers);

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);

        return document.WithSyntaxRoot(newRoot);
    }
}