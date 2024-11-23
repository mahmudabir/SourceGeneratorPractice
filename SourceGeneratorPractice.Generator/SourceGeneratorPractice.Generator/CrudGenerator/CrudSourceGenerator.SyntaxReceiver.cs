using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneratorPractice.Generator;

public enum SourceType
{
    Service,
    Repository,
    Controller
}

public class CrudRegistration(
    TypeSyntax? DbContextType,
    TypeSyntax? CreateRequestType,
    TypeSyntax? UpdateRequestType,
    TypeSyntax? DtoType,
    TypeSyntax EntityType,
    TypeSyntax KeyType,
    string? Route,
    SourceType SourceType
)
{
    public TypeSyntax? DbContextType { get; } = DbContextType;
    public TypeSyntax? CreateRequestType { get; } = CreateRequestType;
    public TypeSyntax? UpdateRequestType { get; } = UpdateRequestType;
    public TypeSyntax? DtoType { get; } = DtoType;
    public TypeSyntax EntityType { get; } = EntityType;
    public TypeSyntax KeyType { get; } = KeyType;
    public string? Route { get; } = Route;

    public SourceType SourceType { get; } = SourceType;

}

public class CrudSyntaxReceiver : ISyntaxReceiver
{
    public List<CrudRegistration> Registrations { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // Look for AddModern method calls
        if (syntaxNode is InvocationExpressionSyntax invocation
          &&
            invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            // &&
            //   memberAccess.Name.Identifier.Text == "AddModern")
        {
            // Process the chained method calls starting from AddModern
            ProcessAddModern(invocation);
        }
    }

    private void ProcessAddModern(InvocationExpressionSyntax addModernInvocation)
    {
        var currentInvocation = addModernInvocation;

        while (currentInvocation != null)
        {
            if (currentInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var methodName = memberAccess.Name.Identifier.Text;

                // Check for specific methods in the chain
                if (methodName == "AddRepositories")
                {
                    ProcessRepositories(currentInvocation);
                }
                else if (methodName == "AddServices")
                {
                    ProcessServices(currentInvocation);
                }
                else if (methodName == "AddControllers")
                {
                    ProcessControllers(currentInvocation);
                }

                // Move to the next method in the chain
                currentInvocation = memberAccess.Expression as InvocationExpressionSyntax;
            }
            else
            {
                break;
            }
        }
    }

    private void ProcessRepositories(InvocationExpressionSyntax invocation)
    {
        var lambdaArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (lambdaArgument?.Expression is SimpleLambdaExpressionSyntax lambda &&
            lambda.Body is BlockSyntax block)
        {
            foreach (var statement in block.Statements.OfType<ExpressionStatementSyntax>())
            {
                if (statement.Expression is InvocationExpressionSyntax repoCall &&
                    repoCall.Expression is MemberAccessExpressionSyntax repoAccess &&
                    repoAccess.Name is GenericNameSyntax genericName &&
                    genericName.Identifier.Text == "AddRepository")
                {
                    // Extract generic arguments
                    var genericArguments = genericName.TypeArgumentList.Arguments;

                    if (genericArguments.Count == 3)
                    {
                        var dbContextType = genericArguments[0];
                        var entityType = genericArguments[1];
                        var keyType = genericArguments[2];

                        // Add registration for repository
                        AddRegistration(new CrudRegistration(
                                                             DbContextType: dbContextType,
                                                             CreateRequestType: null,
                                                             UpdateRequestType: null,
                                                             DtoType: null,
                                                             EntityType: entityType,
                                                             KeyType: keyType,
                                                             Route: null,
                                                             SourceType: SourceType.Repository
                                                            ));
                    }
                }
            }
        }
    }

    private void ProcessServices(InvocationExpressionSyntax invocation)
    {
        var lambdaArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (lambdaArgument?.Expression is SimpleLambdaExpressionSyntax lambda &&
            lambda.Body is BlockSyntax block)
        {
            foreach (var statement in block.Statements.OfType<ExpressionStatementSyntax>())
            {
                if (statement.Expression is InvocationExpressionSyntax serviceCall &&
                    serviceCall.Expression is MemberAccessExpressionSyntax serviceAccess &&
                    serviceAccess.Name is GenericNameSyntax genericName &&
                    genericName.Identifier.Text == "AddService")
                {
                    // Extract generic arguments
                    var genericArguments = genericName.TypeArgumentList.Arguments;

                    if (genericArguments.Count == 3)
                    {
                        var dtoType = genericArguments[0];
                        var entityType = genericArguments[1];
                        var keyType = genericArguments[2];

                        // Add registration for service
                        AddRegistration(new CrudRegistration(
                                                             DbContextType: null,
                                                             CreateRequestType: null,
                                                             UpdateRequestType: null,
                                                             DtoType: dtoType,
                                                             EntityType: entityType,
                                                             KeyType: keyType,
                                                             Route: null,
                                                             SourceType: SourceType.Service
                                                            ));
                    }
                }
            }
        }
    }

    private void ProcessControllers(InvocationExpressionSyntax invocation)
    {
        var lambdaArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (lambdaArgument?.Expression is SimpleLambdaExpressionSyntax lambda &&
            lambda.Body is BlockSyntax block)
        {
            foreach (var statement in block.Statements.OfType<ExpressionStatementSyntax>())
            {
                if (statement.Expression is InvocationExpressionSyntax controllerCall &&
                    controllerCall.Expression is MemberAccessExpressionSyntax controllerAccess &&
                    controllerAccess.Name is GenericNameSyntax genericName &&
                    genericName.Identifier.Text == "AddController")
                {
                    // Extract generic arguments
                    var genericArguments = genericName.TypeArgumentList.Arguments;

                    if (genericArguments.Count == 5)
                    {
                        var createRequestType = genericArguments[0];
                        var updateRequestType = genericArguments[1];
                        var dtoType = genericArguments[2];
                        var entityType = genericArguments[3];
                        var keyType = genericArguments[4];
                        var route = controllerCall.ArgumentList.Arguments[0].ToString().Trim('"');

                        // Add registration for controller
                        AddRegistration(new CrudRegistration(
                                                             DbContextType: null,
                                                             CreateRequestType: createRequestType,
                                                             UpdateRequestType: updateRequestType,
                                                             DtoType: dtoType,
                                                             EntityType: entityType,
                                                             KeyType: keyType,
                                                             Route: route,
                                                             SourceType: SourceType.Controller
                                                            ));
                    }
                }
            }
        }
    }

    private void AddRegistration(CrudRegistration registration)
    {
        if (!Registrations.Contains(registration))
        {
            Registrations.Add(registration);
        }
    }
}
