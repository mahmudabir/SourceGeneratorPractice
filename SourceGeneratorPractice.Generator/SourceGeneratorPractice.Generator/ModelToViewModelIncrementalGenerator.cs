using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorPractice.Generator;

[Generator]
public class ModelToViewModelIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all class declarations with the "HasViewModel" attribute
        var classDeclarations = context.SyntaxProvider
                                       .CreateSyntaxProvider(
                                                             predicate: static (syntaxNode, _) => IsCandidateClass(syntaxNode),
                                                             transform: static (syntaxContext, _) => GetClassDeclarationSyntax(syntaxContext))
                                       .Where(static classDeclaration => classDeclaration != null);

        // Combine the class declarations with the semantic model
        var classesWithModels = classDeclarations
                                .Combine(context.CompilationProvider)
                                .Select(static (pair, _) => GetSemanticClassData(pair.Left, pair.Right))
                                .Where(static semanticData => semanticData != null);

        // Generate the ViewModel and Mapping classes
        context.RegisterSourceOutput(classesWithModels, static (sourceContext, semanticData) =>
        {
            if (semanticData is not (INamedTypeSymbol classSymbol, string namespaceName, string className, string dtoClassName, List<IPropertySymbol> properties))
                return;

            // Generate ViewModel class
            var dtoSource = GenerateViewModelClass(namespaceName, dtoClassName, properties);
            sourceContext.AddSource($"{dtoClassName}.g.cs", SourceText.From(dtoSource, Encoding.UTF8));

            // Generate Mapping class
            var mappingSource = GenerateModelMappingClass(namespaceName, className, dtoClassName, properties);
            sourceContext.AddSource($"{className}_MappingExtensions.g.cs", SourceText.From(mappingSource, Encoding.UTF8));
        });
    }

    private static bool IsCandidateClass(SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.AttributeLists
                               .SelectMany(attrList => attrList.Attributes)
                               .Any(attr => attr.Name.ToString() == "HasViewModel");
    }

    private static ClassDeclarationSyntax? GetClassDeclarationSyntax(GeneratorSyntaxContext context)
    {
        // var classDeclaration = (ClassDeclarationSyntax)context.Node;
        // var hasViewModelAttribute = IsCandidateClass(context.Node);

        return IsCandidateClass(context.Node) ? (ClassDeclarationSyntax) context.Node : null;
    }

    private static (INamedTypeSymbol classSymbol, string namespaceName, string className, string dtoClassName, List<IPropertySymbol> properties)?
        GetSemanticClassData(ClassDeclarationSyntax? classDeclaration, Compilation compilation)
    {
        if (classDeclaration == null) return null;

        var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol == null) return null;

        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var dtoClassName = $"{className}ViewModel";

        var properties = classSymbol.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                                    .ToList();

        return (classSymbol, namespaceName, className, dtoClassName, properties);
    }

    private static string GenerateViewModelClass(string namespaceName, string dtoClassName, List<IPropertySymbol> properties)
    {
        var propertyDeclarations = string.Join("\n", properties.Select(p =>
                                                                           $"        public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"
namespace {namespaceName}
{{
    public class {dtoClassName}
    {{
{propertyDeclarations}
    }}
}}";
    }

    private static string GenerateModelMappingClass(string namespaceName, string className, string dtoClassName, List<IPropertySymbol> properties)
    {
        var assignments = string.Join("\n", properties.Select(p =>
                                                                  $"                {p.Name} = model.{p.Name},"));

        return $@"
namespace {namespaceName}
{{
    public static class {className}MappingExtensions
    {{
        public static {dtoClassName} ToViewModel(this {className} model)
        {{
            return new {dtoClassName}
            {{
{assignments}
            }};
        }}

        public static {className} ToModel(this {dtoClassName} model)
        {{
            return new {className}
            {{
{assignments}
            }};
        }}
    }}
}}";
    }
}
