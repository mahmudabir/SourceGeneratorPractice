using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorPractice.Generator;

[Generator]
public class CrudSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new CrudSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not CrudSyntaxReceiver receiver)
            return;

        // Use Compilation to get the SemanticModel
        var compilation = context.Compilation;

        foreach (var registration in receiver.Registrations)
        {
            var dbContextType = GetFullTypeName(compilation, registration.DbContextType)?.Replace("global::", "");
            var entityType = GetFullTypeName(compilation, registration.EntityType)?.Replace("global::", "");
            var keyType = GetFullTypeName(compilation, registration.KeyType)?.Replace("global::", "");
            var dtoType = GetFullTypeName(compilation, registration.DtoType)?.Replace("global::", "");
            var createRequestType = GetFullTypeName(compilation, registration.CreateRequestType)?.Replace("global::", "");
            var updateRequestType = GetFullTypeName(compilation, registration.UpdateRequestType)?.Replace("global::", "");

            var route = registration.Route;
            var sourceType = registration.SourceType;

            if (true)
            {
                switch (sourceType)
                {
                    case SourceType.Controller:
                        // Generate Controller

                        try
                        {
                            var controllerSource = GenerateController(createRequestType, updateRequestType, dtoType, entityType, keyType, route);
                            context.AddSource($"{entityType.Split('.').LastOrDefault()}Controller.g.cs", SourceText.From(controllerSource, Encoding.UTF8));
                        }
                        catch (Exception e)
                        {
                            // Console.WriteLine(e);
                            // throw;
                        }
                        break;
                    case SourceType.Service:
                        // Generate Service

                        try
                        {
                            var serviceSource = GenerateService(dtoType, entityType, keyType);
                            context.AddSource($"{entityType.Split('.').LastOrDefault()}Service.g.cs", SourceText.From(serviceSource, Encoding.UTF8));
                        }
                        catch (Exception e)
                        {
                            // Console.WriteLine(e);
                            // throw;
                        }

                        break;
                    case SourceType.Repository:
                        // Generate Repository

                        try
                        {
                            var repositorySource = GenerateRepository(dbContextType, entityType, keyType);
                            context.AddSource($"{entityType.Split('.').LastOrDefault()}Repository.g.cs", SourceText.From(repositorySource, Encoding.UTF8));
                        }
                        catch (Exception e)
                        {
                            // Console.WriteLine(e);
                            // throw;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Resolves the full type name (including namespace) from a TypeSyntax using SemanticModel.
    /// </summary>
    private string? GetFullTypeName(Compilation compilation, TypeSyntax? typeSyntax, string? fallbackNamespace = null)
    {
        if (typeSyntax == null)
            return null;

        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
        var typeInfo = semanticModel.GetTypeInfo(typeSyntax);

        // Ensure the type is an INamedTypeSymbol
        if (typeInfo.Type is INamedTypeSymbol namedTypeSymbol)
        {
            // Return the fully qualified name
            return namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        // Handle cases where the type could not be resolved by falling back
        if (fallbackNamespace != null)
        {
            return $"{fallbackNamespace}.{typeSyntax}";
        }

        return typeSyntax.ToString(); // Return raw type as a fallback
    }

    private string GenerateRepository(string dbContextType, string entityType, string keyType)
    {
        var splitValues = entityType.Split('.').ToList();
        splitValues.RemoveRange(splitValues.Count - 1, 1);
        
        return $@"
using System.Collections.Generic;
using {string.Join(".", splitValues)};
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Generated
{{
    public class {entityType.Split('.').LastOrDefault()}Repository
    {{
        private readonly {dbContextType} _dbContext;

        public {entityType.Split('.').LastOrDefault()}Repository({dbContextType} dbContext)
        {{
            _dbContext = dbContext;
        }}

        public async Task<{entityType}?> GetByIdAsync({keyType} id) => await _dbContext.Set<{entityType}>().FindAsync(id);

        public async Task<IEnumerable<{entityType}>> GetAllAsync() => await _dbContext.Set<{entityType}>().ToListAsync();

        public async Task AddAsync({entityType} entity)
        {{
            await _dbContext.Set<{entityType}>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }}

        public async Task UpdateAsync({entityType} entity)
        {{
            _dbContext.Set<{entityType}>().Update(entity);
            await _dbContext.SaveChangesAsync();
        }}

        public async Task DeleteAsync({keyType} id)
        {{
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {{
                _dbContext.Set<{entityType}>().Remove(entity);
                await _dbContext.SaveChangesAsync();
            }}
        }}
    }}
}}";
    }

    private string GenerateService(string dtoType, string entityType, string keyType)
    {
        var splitValues = dtoType.Split('.').ToList();
        splitValues.RemoveRange(splitValues.Count - 1, 1);

        return $@"
using System.Linq;
using {string.Join(".", splitValues)};
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Generated
{{
    public class {entityType.Split('.').LastOrDefault()}Service
    {{
        private readonly {entityType.Split('.').LastOrDefault()}Repository _repository;

        public {entityType.Split('.').LastOrDefault()}Service({entityType.Split('.').LastOrDefault()}Repository repository)
        {{
            _repository = repository;
        }}

        public async Task<IEnumerable<{dtoType}>> GetAllAsync()
        {{
            var entities = await _repository.GetAllAsync();
            //return entities.Select(e => new {dtoType} {{ /* Map properties */ }}).ToList();
            return entities.Select(e => e.ToViewModel()).ToList();
        }}

        public async Task<{dtoType}?> GetByIdAsync({keyType} id)
        {{
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : entity.ToViewModel(); //new {dtoType} {{ /* Map properties */ }};
        }}

        public async Task AddAsync({dtoType} dto)
        {{
            var entity = dto.ToModel(); //new {entityType} {{ /* Map properties */ }};
            await _repository.AddAsync(entity);
        }}

        public async Task UpdateAsync({dtoType} dto)
        {{
            var entity = dto.ToModel(); //new {entityType} {{ /* Map properties */ }};
            await _repository.UpdateAsync(entity);
        }}

        public async Task DeleteAsync({keyType} id) => await _repository.DeleteAsync(id);
    }}
}}";
    }

    private string GenerateController(string createRequestType, string updateRequestType, string dtoType, string entityType, string keyType, string route)
    {
        var splitValues = dtoType.Split('.').ToList();
        splitValues.RemoveRange(splitValues.Count - 1, 1);
        
        return $@"
using System.Collections.Generic;
using System.Threading.Tasks;
using {string.Join(".", splitValues)};
using Microsoft.AspNetCore.Mvc;

namespace Generated
{{
    [ApiController]
    [Route(""{route}"")]
    public class {entityType.Split('.').LastOrDefault()}Controller : ControllerBase
    {{
        private readonly {entityType.Split('.').LastOrDefault()}Service _service;

        public {entityType.Split('.').LastOrDefault()}Controller({entityType.Split('.').LastOrDefault()}Service service)
        {{
            _service = service;
        }}

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet(""{entityType.Split('.').LastOrDefault().ToLower()}/{{id}}"")]
        public async Task<IActionResult> GetById({keyType} id)
        {{
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }}

        [HttpPost]
        public async Task<IActionResult> Create({createRequestType} request)
        {{
            await _service.AddAsync(request);
            return CreatedAtAction(nameof(GetById), new {{ id = """" }}, null);
        }}

        [HttpPut]
        public async Task<IActionResult> Update({updateRequestType} request)
        {{
            await _service.UpdateAsync(request);
            return NoContent();
        }}

        [HttpDelete(""{entityType.Split('.').LastOrDefault().ToLower()}/{{id}}"")]
        public async Task<IActionResult> Delete({keyType} id)
        {{
            await _service.DeleteAsync(id);
            return NoContent();
        }}
    }}
}}";
    }
}
