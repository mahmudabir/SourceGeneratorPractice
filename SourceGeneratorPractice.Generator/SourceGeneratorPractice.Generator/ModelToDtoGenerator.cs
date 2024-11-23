using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorPractice.Generator;

[Generator]
public class ModelToDtoGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol == null)
                continue;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var dtoClassName = $"{className}Dto";

            var properties = classSymbol.GetMembers()
                                        .OfType<IPropertySymbol>()
                                        .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                                        .ToList();

            var dtoSource = GenerateDtoClass(namespaceName, dtoClassName, properties);
            context.AddSource($"{dtoClassName}.g.cs", SourceText.From(dtoSource, Encoding.UTF8));

            var mappingSource = GenerateDtoMappingClass(namespaceName, className, dtoClassName, properties);
            context.AddSource($"{className}_DtoMappingExtensions.g.cs", SourceText.From(mappingSource, Encoding.UTF8));
        }
    }

    private string GenerateDtoClass(string namespaceName, string dtoClassName, List<IPropertySymbol> properties)
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

    private string GenerateDtoMappingClass(string namespaceName, string className, string dtoClassName, List<IPropertySymbol> properties)
    {
        var assignments = string.Join("\n", properties.Select(p =>
                                                                  $"                {p.Name} = model.{p.Name},"));

        return $@"
namespace {namespaceName}
{{
    public static class {className}DtoMappingExtensions
    {{
        public static {dtoClassName} ToDto(this {className} model)
        {{
            return new {dtoClassName}
            {{
{assignments}
            }};
        }}
    }}
}}";
    }
}

public class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.AttributeLists.Count > 0)
        {
            var hasGenerateDtoAttribute = classDeclaration.AttributeLists
                                                          .SelectMany(attrList => attrList.Attributes)
                                                          .Any(attr => attr.Name.ToString() == "HasViewModel");

            if (hasGenerateDtoAttribute)
            {
                CandidateClasses.Add(classDeclaration);
            }
        }
    }
}
