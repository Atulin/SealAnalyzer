using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SealAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MakeTypeSealedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SEAL001";
        private static readonly LocalizableString Title = "Type can be sealed";
        private static readonly LocalizableString MessageFormat = "Type '{0}' can be sealed";
        private const string Category = "Design";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationStartAnalysisContext context)
        {
            var attributeSymbol = context.Compilation.GetTypeByMetadataName($"{SealPublicClassesGenerator.Namespace}.{SealPublicClassesGenerator.AttributeName}");

            var isOptIn = false;
            if (attributeSymbol != null)
            {
                var assemblyAttributes = context.Compilation.Assembly.GetAttributes();
                isOptIn = assemblyAttributes.Any(attr => 
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol));
            }

            context.RegisterSyntaxNodeAction(ctx => AnalyzeType(ctx, isOptIn), 
                SyntaxKind.ClassDeclaration, 
                SyntaxKind.RecordDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context, bool isOptIn)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

            if (symbol == null || symbol.TypeKind != TypeKind.Class) return;
            if (symbol.IsSealed || symbol.IsAbstract || symbol.IsStatic) return;

            var shouldAnalyze = symbol.DeclaredAccessibility == Accessibility.Private 
                                || symbol.DeclaredAccessibility == Accessibility.Internal 
                                || symbol.DeclaredAccessibility == Accessibility.Public 
                                && isOptIn;

            if (shouldAnalyze)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(), symbol.Name));
            }
        }
    }
}